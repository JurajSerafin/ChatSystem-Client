using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Ids;
using ChatSystem.Client.Infrastructure.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace ChatSystem.Client.Tests.Infrastructure.Repositories;

public class LocalUserRepositoryTests : RepositoryTestBase {
    private readonly LocalUserRepository _repo;

    public LocalUserRepositoryTests() {
        _repo = new LocalUserRepository(DbContext);
    }

    private static CachedUser NewUser(
        UserId? id = null,
        string login = "user",
        string tag = "tag"
    ) {
        return new CachedUser {
            Id = id ?? IdFactory.Generate<UserId>(),
            Login = login,
            Tag = tag,
            PublicKey = "pubkey",
            Role = "user",
            CreatedAt = default
        };
    }

    [Fact]
    public async Task FindByIdAsync_ExistingUser_ReturnsUser() {
        // Arrange
        var user = NewUser(login: "alice");
        await _repo.UpsertAsync(user);

        // Act
        var result = await _repo.FindByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("alice", result.Login);
    }

    [Fact]
    public async Task FindByIdAsync_NonExistingUser_ReturnsNull() {
        // Arrange & Act
        var result = await _repo.FindByIdAsync(IdFactory.Generate<UserId>());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByIdAsync_ReturnsCorrectUser_WhenMultipleExist() {
        // Arrange
        var alice = NewUser(login: "alice");
        var bob = NewUser(login: "bob");

        await _repo.UpsertAsync(alice);
        await _repo.UpsertAsync(bob);

        // Act
        var result = await _repo.FindByIdAsync(bob.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("bob", result.Login);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_MatchesByLogin() {
        // Arrange
        await _repo.UpsertAsync(NewUser(login: "alice", tag: "#123"));
        await _repo.UpsertAsync(NewUser(login: "bob", tag: "#456"));

        // Act
        var result = await _repo.FindByLoginOrTagAsync("alice", limit: 10, offset: 0);

        // Assert
        Assert.Single(result);
        Assert.Equal("alice", result[0].Login);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_MatchesByTag() {
        // Arrange
        await _repo.UpsertAsync(NewUser(login: "l1", tag: "#alice123"));
        await _repo.UpsertAsync(NewUser(login: "l2", tag: "#bob456"));

        // Act
        var result = await _repo.FindByLoginOrTagAsync("alice", limit: 10, offset: 0);

        // Assert
        Assert.Single(result);
        Assert.Contains("alice", result[0].Tag);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_IsCaseInsensitive_ForLogin() {
        // Arrange
        await _repo.UpsertAsync(NewUser(login: "Alice", tag: "#alice"));

        // Act
        var result = await _repo.FindByLoginOrTagAsync("ALICE", limit: 10, offset: 0);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_IsCaseInsensitive_ForTag() {
        // Arrange
        await _repo.UpsertAsync(NewUser(login: "user", tag: "#MyTag"));

        // Act
        var result = await _repo.FindByLoginOrTagAsync("mytag", limit: 10, offset: 0);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_PartialMatch_ReturnsUser() {
        // Arrange
        await _repo.UpsertAsync(NewUser(login: "alice", tag: "#123"));

        // Act
        var result = await _repo.FindByLoginOrTagAsync("al", limit: 10, offset: 0);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_NoMatches_ReturnsEmpty() {
        // Arrange
        await _repo.UpsertAsync(NewUser(login: "alice"));

        // Act
        var result = await _repo.FindByLoginOrTagAsync("xxx", limit: 10, offset: 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_EmptyDatabase_ReturnsEmpty() {
        // Arrange & Act
        var result = await _repo.FindByLoginOrTagAsync("abc", limit: 10, offset: 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_RespectsLimit() {
        // Arrange
        const string testLoginPrefix = "test_user";
        const int findLimit = 2;

        for (var i = 0; i < 5; i++) {
            await _repo.UpsertAsync(NewUser(login: $"{testLoginPrefix}{i}", tag: $"#tag{i}"));
        }

        // Act
        var result = await _repo.FindByLoginOrTagAsync(testLoginPrefix, limit: findLimit, offset: 0);

        // Assert
        Assert.Equal(findLimit, result.Count);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_RespectsOffset() {
        // Arrange
        const string testLoginPrefix = "test_user";
        const int findLimit = 2;

        for (var i = 0; i < 5; i++) {
            await _repo.UpsertAsync(NewUser(login: $"{testLoginPrefix}{i}", tag: $"#tag{i}"));
        }
        // Act
        var page1 = await _repo.FindByLoginOrTagAsync(testLoginPrefix, limit: findLimit,
            offset: 0);
        
        var page2 = await _repo.FindByLoginOrTagAsync(testLoginPrefix, limit: findLimit,
            offset: findLimit);

        // Assert
        Assert.Equal(findLimit, page1.Count);
        Assert.Equal(findLimit, page2.Count);

        var pagesIntersection = page1.Select(u => u.Id).Intersect(
            page2.Select(u => u.Id));
        Assert.Empty(pagesIntersection);
    }

    [Fact]
    public async Task FindByLoginOrTagAsync_OrdersByLogin() {
        // Arrange
        await _repo.UpsertAsync(NewUser(login: "charlie", tag: "#t3"));
        await _repo.UpsertAsync(NewUser(login: "alice", tag: "#t1"));
        await _repo.UpsertAsync(NewUser(login: "bob", tag: "#t2"));

        // Act
        var result = await _repo.FindByLoginOrTagAsync("", limit: 10, offset: 0);

        // Assert
        Assert.Equal("alice", result[0].Login);
        Assert.Equal("bob", result[1].Login);
        Assert.Equal("charlie", result[2].Login);
    }


    [Fact]
    public async Task UpsertAsync_NewUser_InsertsIntoDatabase() {
        // Arrange
        var user = NewUser(login: "new");

        // Act
        await _repo.UpsertAsync(user);

        var result = await _repo.FindByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpsertAsync_UpdatesFields() {
        // Arrange
        var user = NewUser(login: "original");
        await _repo.UpsertAsync(user);

        user.Login = "updated";

        // Act
        await _repo.UpsertAsync(user);

        var result = await _repo.FindByIdAsync(user.Id);

        // Assert
        Assert.Equal("updated", result!.Login);
    }


    [Fact]
    public async Task UpsertAsync_CreatesNoDuplicates() {

        // Arrange
        var user = NewUser(login: "duplicate");

        // Act
        await _repo.UpsertAsync(user);
        await _repo.UpsertAsync(user);

        var result = await _repo.FindByLoginOrTagAsync("duplicate", limit: 10, offset: 0);
        
        // Assert
        Assert.Single(result);
    }


    [Fact]
    public async Task DeleteAsync_RemovesFromDatabase() {
        // Arrange
        var user = NewUser();
        await _repo.UpsertAsync(user);

        // Act
        await _repo.DeleteAsync(user.Id);

        ClearTracker();

        var result = await _repo.FindByIdAsync(user.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingUser_DoesNotThrow() {
        // Arrange & Act & Assert
        await _repo.DeleteAsync(IdFactory.Generate<UserId>());
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersUnaffected() {
        // Arrange
        var toDelete = NewUser(login: "DELETE_ME");
        var toKeep = NewUser(login: "KEEP_ME");

        await _repo.UpsertAsync(toDelete);
        await _repo.UpsertAsync(toKeep);

        // Act
        await _repo.DeleteAsync(toDelete.Id);
        ClearTracker();

        var result = await _repo.FindByIdAsync(toKeep.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(toKeep.Id, result.Id);
    }
}