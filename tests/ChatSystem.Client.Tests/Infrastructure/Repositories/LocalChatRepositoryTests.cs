using System;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Ids;
using ChatSystem.Client.Infrastructure.Repositories;

namespace ChatSystem.Client.Tests.Infrastructure.Repositories;

public class LocalChatRepositoryTests : RepositoryTestBase {
    private readonly LocalChatRepository _repo;

    public LocalChatRepositoryTests() {
        _repo = new LocalChatRepository(DbContext);
    }

    private static CachedChat NewChat(string? name = null, DateTimeOffset? lastActivity = null) {
        return new CachedChat {
            Id = IdFactory.Generate<ChatId>(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
            LastActivityAt = lastActivity ?? DateTimeOffset.UtcNow,
            CachedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };
    }

    private static CachedUser NewUser(UserId? id = null, string login = "user") {
        return new CachedUser {
            Id = id ?? IdFactory.Generate<UserId>(),
            Login = login,
            Tag = "#" + login,
            PublicKey = "pubkey-here",
            Role = "user",
            CreatedAt = new DateTimeOffset()
        };
    }
    protected async Task<UserId> SeedUserAsync(string login = "user") {
        var user = NewUser(login: login);

        DbContext.Users.Add(user);

        await DbContext.SaveChangesAsync();

        return user.Id;
    }

    [Fact]
    public async Task FindByIdAsync_ExistingChat_ReturnsChat() {
        // Arrange
        var chat = NewChat("Test Chat");
        await _repo.UpsertAsync(chat);

        // Act
        var result = await _repo.FindByIdAsync(chat.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(chat.Id, result.Id);
        Assert.Equal("Test Chat", result.Name);
    }

    [Fact]
    public async Task FindByIdAsync_NonExistingChat_ReturnsNull() {
        // Arrange & Act
        var result = await _repo.FindByIdAsync(IdFactory.Generate<ChatId>());

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task FindAllAsync_EmptyDatabase_ReturnsEmptyList() {
        // Arrange & Act
        var result = await _repo.FindAllAsync(limit: 10, offset: 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindAllAsync_MultipleChats_ReturnsAllWithinLimit() {
        // Arrange
        const int testUpsertCount = 5;

        for (var i = 0; i < testUpsertCount; i++) {
            await _repo.UpsertAsync(NewChat($"Chat {i}"));
        }

        // Act
        var result = await _repo.FindAllAsync(limit: 10, offset: 0);

        // Assert
        Assert.Equal(testUpsertCount, result.Count);
    }

    [Fact]
    public async Task FindAllAsync_OrdersByLastActivityDescending() {
        // Arrange
        var oldest = NewChat("Oldest", DateTimeOffset.UtcNow.AddDays(-3));
        var middle = NewChat("Middle", DateTimeOffset.UtcNow.AddDays(-2));
        var newest = NewChat("Newest", DateTimeOffset.UtcNow.AddDays(-1));

        await _repo.UpsertAsync(oldest);
        await _repo.UpsertAsync(middle);
        await _repo.UpsertAsync(newest);

        // Act
        var result = await _repo.FindAllAsync(limit: 10, offset: 0);

        // Assert
        Assert.Equal(newest.Id, result[0].Id);
        Assert.Equal(middle.Id, result[1].Id);
        Assert.Equal(oldest.Id, result[2].Id);
    }

    [Fact]
    public async Task FindAllAsync_LimitWorksCorrectly() {
        // Arrange
        const int testUpsertCount = 5;
        const int findCount = 2;

        for (var i = 0; i < testUpsertCount; i++) {
            await _repo.UpsertAsync(NewChat($"Chat {i}"));
        }

        // Act
        var result = await _repo.FindAllAsync(limit: findCount, offset: 0);

        // Assert
        Assert.Equal(findCount, result.Count);
    }

    [Fact]
    public async Task FindAllAsync_RespectsOffset() {
        const int testUpsertCount = 5;
        const int findCount = 2;

        for (var i = 0; i < testUpsertCount; i++) {
            await _repo.UpsertAsync(NewChat($"Chat {i}", DateTimeOffset.UtcNow.AddSeconds(i)));
        }

        var page1 = await _repo.FindAllAsync(limit: findCount, offset: 0);
        var page2 = await _repo.FindAllAsync(limit: findCount, offset: findCount);

        Assert.NotEqual(page1[0].Id, page2[0].Id);
        Assert.NotEqual(page1[1].Id, page2[1].Id);
    }

    [Fact]
    public async Task UpsertAsync_NewChat_InsertsIntoDatabase() {
        // Arrange
        var chat = NewChat("new chat");

        // Act
        await _repo.UpsertAsync(chat);

        var result = await _repo.FindByIdAsync(chat.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new chat", result.Name);
    }

    [Fact]
    public async Task UpsertAsync_ExistingChat_UpdatesInDatabase() {
        // Arrange
        var chat = NewChat("original");
        await _repo.UpsertAsync(chat);

        // Act
        chat.Name = "updated";
        await _repo.UpsertAsync(chat);

        // Assert
        var result = await _repo.FindByIdAsync(chat.Id);
        Assert.Equal("updated", result!.Name);
    }

    [Fact]
    public async Task UpsertAsync_ExistingChat_DoesNotCreateDuplicate() {
        // Arrange
        var chat = NewChat("test");

        // Act
        await _repo.UpsertAsync(chat);
        await _repo.UpsertAsync(chat);

        // Assert
        var all = await _repo.FindAllAsync(limit: 10, offset: 0);
        Assert.Single(all);
    }

    [Fact]
    public async Task DeleteAsync_ExistingChat_RemovesFromDatabase() {
        // Arrange
        var chat = NewChat("chat to delete");
        await _repo.UpsertAsync(chat);

        // Act
        await _repo.DeleteAsync(chat.Id);
        ClearTracker();

        // Assert
        var result = await _repo.FindByIdAsync(chat.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingChat_DoesNotThrow() {
        // Arrange & Act & Assert
        await _repo.DeleteAsync(IdFactory.Generate<ChatId>());
    }

    [Fact]
    public async Task AddParticipantAsync_ValidData_PersistsParticipant() {
        // Arrange
        var chat = NewChat("test");
        await _repo.UpsertAsync(chat);
        var userId = await SeedUserAsync();

        // Act
        await _repo.AddParticipantAsync(userId, chat.Id, "member");

        // Assert
        var ids = await _repo.GetParticipantIdsAsync(chat.Id);
        Assert.Contains(userId, ids);
    }


    [Fact]
    public async Task GetParticipantIdsAsync_NoParticipants_ReturnsEmpty() {
        // Arrange
        var chat = NewChat("empty");
        await _repo.UpsertAsync(chat);

        // Act
        var result = await _repo.GetParticipantIdsAsync(chat.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetParticipantIdsAsync_MultipleParticipants_ReturnsAll() {
        // Arrange
        var chat = NewChat("Group");
        await _repo.UpsertAsync(chat);
        const int addedParticipantsCount = 3;

        var user1 = await SeedUserAsync("user1");
        var user2 = await SeedUserAsync("user2");
        var user3 = await SeedUserAsync("user3");

        await _repo.AddParticipantAsync(user1, chat.Id, "member");
        await _repo.AddParticipantAsync(user2, chat.Id, "member");
        await _repo.AddParticipantAsync(user3, chat.Id, "admin");

        // Act
        var result = await _repo.GetParticipantIdsAsync(chat.Id);

        // Assert
        Assert.Equal(addedParticipantsCount, result.Count);
        Assert.Contains(user1, result);
        Assert.Contains(user2, result);
        Assert.Contains(user3, result);
    }

    [Fact]
    public async Task RemoveParticipantAsync_ExistingParticipant_RemovesFromChat() {
        var chat = NewChat("Test");
        await _repo.UpsertAsync(chat);
        var userId = await SeedUserAsync();

        await _repo.AddParticipantAsync(userId, chat.Id, "member");
        await _repo.RemoveParticipantAsync(userId, chat.Id);

        var ids = await _repo.GetParticipantIdsAsync(chat.Id);
        Assert.DoesNotContain(userId, ids);
    }

    [Fact]
    public async Task RemoveParticipantAsync_OtherParticipantsUnaffected() {
        var chat = NewChat("Test");
        await _repo.UpsertAsync(chat);
        var userToRemove = await SeedUserAsync("toremove");
        var userToKeep = await SeedUserAsync("tokeep");

        await _repo.AddParticipantAsync(userToRemove, chat.Id, "member");
        await _repo.AddParticipantAsync(userToKeep, chat.Id, "member");
        await _repo.RemoveParticipantAsync(userToRemove, chat.Id);

        var ids = await _repo.GetParticipantIdsAsync(chat.Id);
        Assert.Single(ids);
        Assert.Contains(userToKeep, ids);
    }
}