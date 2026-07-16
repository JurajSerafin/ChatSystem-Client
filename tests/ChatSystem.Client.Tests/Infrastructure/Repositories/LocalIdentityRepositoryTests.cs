using System;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Identity;
using ChatSystem.Client.Core.Interfaces.Ids;
using ChatSystem.Client.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Client.Tests.Infrastructure.Repositories;

public class LocalIdentityRepositoryTests : RepositoryTestBase {
    private readonly LocalIdentityRepository _repo;

    public LocalIdentityRepositoryTests() {
        _repo = new LocalIdentityRepository(DbContext);
    }

    private static CachedIdentity NewIdentity(UserId? id = null, string token = "test-session-token") {
        return new CachedIdentity {
            Id = id ?? IdFactory.Generate<UserId>(),
            SessionToken = token,
            Login = "test_user",
            Tag = "123",
        };
    }

    [Fact]
    public async Task StoreAsync_EmptyTable_InsertsIdentity() {

        // Arrange
        var identity = NewIdentity(token: "initial-token");

        // Act
        await _repo.StoreAsync(identity);
        var loaded = await _repo.LoadAsync();

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("initial-token", loaded.SessionToken);
    }

    [Fact]
    public async Task StoreAsync_ExistingIdentity_UpdatesRow() {
        // Arrange
        await _repo.StoreAsync(NewIdentity(token: "first-token"));

        // Act
        var newIdentity = NewIdentity(token: "second-token");
        await _repo.StoreAsync(newIdentity);

        // Assert
        var loaded = await _repo.LoadAsync();
        Assert.Equal("second-token", loaded!.SessionToken);
    }

    [Fact]
    public async Task StoreAsync_MultipleCalls_KeepsSingleRow() {
        // Arrange
        await _repo.StoreAsync(NewIdentity(token: "first"));
        await _repo.StoreAsync(NewIdentity(token: "second"));
        await _repo.StoreAsync(NewIdentity(token: "third"));

        // Act & Assert
        var count = await DbContext.Identity.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task StoreAsync_UpdatesUserIdCorrectly() {
        // Arrange
        var firstUserId = IdFactory.Generate<UserId>();
        var secondUserId = IdFactory.Generate<UserId>();

        // Act
        await _repo.StoreAsync(NewIdentity(firstUserId, "token1"));
        await _repo.StoreAsync(NewIdentity(secondUserId, "token2"));

        // Assert
        var loaded = await _repo.LoadAsync();
        Assert.Equal(secondUserId, loaded!.Id);
        Assert.Equal("token2", loaded.SessionToken);
    }

    [Fact]
    public async Task LoadAsync_EmptyTable_ReturnsNull() {
        // Arrange
        var result = await _repo.LoadAsync();

        // Act & Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadAsync_ExistingIdentity_ReturnsCorrectly() {
        // Arrange
        var identity = NewIdentity(token: "my-token");
        await _repo.StoreAsync(identity);

        // Act
        var result = await _repo.LoadAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("my-token", result.SessionToken);
    }

    [Fact]
    public async Task LoadAsync_ReturnsCorrectUserId() {
        // Arrange
        var userId = IdFactory.Generate<UserId>();
        await _repo.StoreAsync(NewIdentity(userId, "token"));

        // Act
        var result = await _repo.LoadAsync();

        // Assert
        Assert.Equal(userId, result!.Id);
    }


    [Fact]
    public async Task UpdateSessionTokenAsync_ExistingIdentity_UpdatesToken() {
        // Arrange
        await _repo.StoreAsync(NewIdentity(token: "original"));

        // Act
        await _repo.UpdateSessionTokenAsync("refreshed");

        ClearTracker();

        // Assert
        var loaded = await _repo.LoadAsync();
        Assert.Equal("refreshed", loaded!.SessionToken);
    }

    [Fact]
    public async Task UpdateSessionTokenAsync_EmptyTable_ThrowsException() {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.UpdateSessionTokenAsync("new-token"));
    }

    [Fact]
    public async Task UpdateSessionTokenAsync_PreservesUserId() {
        // Arrange
        var userId = IdFactory.Generate<UserId>();
        await _repo.StoreAsync(NewIdentity(userId, "original"));

        // Act
        await _repo.UpdateSessionTokenAsync("refreshed");
        ClearTracker();

        // Assert
        var loaded = await _repo.LoadAsync();
        Assert.Equal(userId, loaded!.Id);
    }

    [Fact]
    public async Task ClearAsync_ExistingIdentity_RemovesCorrectly() {
        // Arrange
        await _repo.StoreAsync(NewIdentity());

        // Act
        await _repo.ClearAsync();
        ClearTracker();

        // Assert
        var result = await _repo.LoadAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task ClearAsync_EmptyTable_DoesNotThrow() {
        // Arrange & Act & Assert
        await _repo.ClearAsync();
    }
}