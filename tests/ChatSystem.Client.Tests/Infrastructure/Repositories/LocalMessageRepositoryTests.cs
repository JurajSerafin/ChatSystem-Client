using System;
using System.Linq;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Ids;
using ChatSystem.Client.Infrastructure.Repositories;

namespace ChatSystem.Client.Tests.Infrastructure.Repositories;

public class LocalMessageRepositoryTests : RepositoryTestBase {
    private readonly LocalMessageRepository _repo;

    public LocalMessageRepositoryTests() {
        _repo = new LocalMessageRepository(DbContext);
    }

    private static CachedMessage NewMessage(
        ChatId chatId,
        string plainText = "Hello",
        DateTimeOffset? createdAt = null,
        bool isRead = false
    ) {
        return new CachedMessage {
            Id = IdFactory.Generate<MessageId>(),
            ChatId = chatId,
            SenderId = IdFactory.Generate<UserId>(),
            PlainText = plainText,
            Type = "TEXT",
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            IsRead = isRead,
            IsDelivered = false,
            IsDeleted = false
        };
    }

    private async Task<ChatId> SeedChatAsync() {
        var chat = new CachedChat {
            Id = IdFactory.Generate<ChatId>(),
            CreatedAt = DateTimeOffset.UtcNow,
            LastActivityAt = DateTimeOffset.UtcNow,
            CachedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        DbContext.Chats.Add(chat);
        await DbContext.SaveChangesAsync();

        return chat.Id;
    }

    [Fact]
    public async Task SaveForChatAsync_NewMessage_InsertsCorrectly() {
        // Arrange
        const string content = "Hello World";
        var chatId = await SeedChatAsync();
        var message = NewMessage(chatId, content);

        // Act
        await _repo.SaveForChatAsync(message);

        // Assert
        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);
        Assert.Single(result);
        Assert.Equal(content, result[0].PlainText);
    }

    [Fact]
    public async Task SaveForChatAsync_ExistingMessage_UpdatesCorrectly() {
        // Arrange
        var chatId = await SeedChatAsync();
        var message = NewMessage(chatId, "original");
        await _repo.SaveForChatAsync(message);

        // Act
        message.PlainText = "updated";
        await _repo.SaveForChatAsync(message);

        // Assert
        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);
        Assert.Single(result);
        Assert.Equal("updated", result[0].PlainText);
    }

    [Fact]
    public async Task SaveForChatAsync_ExistingMessage_DoesNotCreateDuplicate() {
        // Arrange
        var chatId = await SeedChatAsync();
        var message = NewMessage(chatId);

        // Act
        await _repo.SaveForChatAsync(message);
        await _repo.SaveForChatAsync(message);

        // Assert
        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);
        Assert.Single(result);
    }

    [Fact]
    public async Task FindByChatIdAsync_NoMessages_ReturnsEmpty() {
        // Arrange
        var chatId = await SeedChatAsync();

        // Act
        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindByChatIdAsync_NonExistingChat_ReturnsEmpty() {
        // Arrange & Act
        var result = await _repo.FindByChatIdAsync(
            IdFactory.Generate<ChatId>(), limit: 10, offset: 0
        );

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindByChatIdAsync_OnlyReturnsMessagesForSpecifiedChat() {
        // Arrange
        var chat1 = await SeedChatAsync();
        var chat2 = await SeedChatAsync();

        const string msg1 = "chat message 1";
        const string msg2 = "chat message 2";

        await _repo.SaveForChatAsync(NewMessage(chat1, msg1));
        await _repo.SaveForChatAsync(NewMessage(chat2, msg2));

        // Act
        var result1 = await _repo.FindByChatIdAsync(chat1, limit: 10, offset: 0);

        // Assert
        Assert.Single(result1);

        Assert.Equal(msg1, result1[0].PlainText);
    }

    [Fact]
    public async Task FindByChatIdAsync_OrdersByCreatedAtDescending() {
        // Arrange
        var chatId = await SeedChatAsync();
        var oldest = NewMessage(chatId, "oldest", DateTimeOffset.UtcNow.AddMinutes(-10));
        var middle = NewMessage(chatId, "middle", DateTimeOffset.UtcNow.AddMinutes(-5));
        var newest = NewMessage(chatId, "newest", DateTimeOffset.UtcNow);

        await _repo.SaveForChatAsync(oldest);
        await _repo.SaveForChatAsync(middle);
        await _repo.SaveForChatAsync(newest);

        // Act
        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);

        // Assert
        Assert.Equal("newest", result[0].PlainText);
        Assert.Equal("middle", result[1].PlainText);
        Assert.Equal("oldest", result[2].PlainText);
    }

    [Fact]
    public async Task FindByChatIdAsync_PaginationReturnsCorrectResults() {
        // Arrange
        var chatId = await SeedChatAsync();
        const int newMessageCount = 10;
        const int pageLimit = newMessageCount / 2;

        for (var i = 0; i < newMessageCount; i++) {
            await _repo.SaveForChatAsync(NewMessage(chatId, $"Msg {i}", DateTimeOffset.UtcNow.AddSeconds(i)));
        }

        // Act
        var page1 = await _repo.FindByChatIdAsync(chatId, limit: pageLimit, offset: 0);
        var page2 = await _repo.FindByChatIdAsync(chatId, limit: pageLimit, offset: 5);

        // Assert
        Assert.Equal(pageLimit, page1.Count);
        Assert.Equal(pageLimit, page2.Count);
        Assert.Empty(page1.Select(m => m.Id).Intersect(page2.Select(m => m.Id)));
    }

    [Fact]
    public async Task FindByChatIdAsync_RespectsLimit() {
        // Arrange
        var chatId = await SeedChatAsync();
        const int fetchLimit = 2;

        for (var i = 0; i < 5; i++) {
            await _repo.SaveForChatAsync(NewMessage(chatId, $"Msg {i}"));
        }

        // Act
        var result = await _repo.FindByChatIdAsync(chatId, limit: fetchLimit, offset: 0);

        // Assert
        Assert.Equal(fetchLimit, result.Count);
    }


    [Fact]
    public async Task SearchAsync_MatchingKeyword_ReturnsMatches() {
        // Arrange
        var chatId = await SeedChatAsync();
        const int helloCount = 2;

        await _repo.SaveForChatAsync(NewMessage(chatId, "Hello world"));
        await _repo.SaveForChatAsync(NewMessage(chatId, "Goodbye world"));
        await _repo.SaveForChatAsync(NewMessage(chatId, "Hello world again"));

        // Act
        var result = await _repo.SearchAsync(chatId, "Hello", limit: 10, offset: 0);

        // Assert
        Assert.Equal(helloCount, result.Count);
    }

    [Fact]
    public async Task SearchAsync_IsCaseInsensitive() {
        // Arrange
        var chatId = await SeedChatAsync();
        await _repo.SaveForChatAsync(NewMessage(chatId, "Hello World"));

        // Act
        var result = await _repo.SearchAsync(chatId, "hello", limit: 10, offset: 0);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_OnlySearchesSpecifiedChat() {
        // Arrange
        var chat1 = await SeedChatAsync();
        var chat2 = await SeedChatAsync();

        await _repo.SaveForChatAsync(NewMessage(chat1, "Hello world"));
        await _repo.SaveForChatAsync(NewMessage(chat2, "Hello world 2"));

        // Act
        var result = await _repo.SearchAsync(chat1, "Hello", limit: 10, offset: 0);

        // Assert
        Assert.Single(result);
        Assert.Equal("Hello world", result[0].PlainText);
    }

    [Fact]
    public async Task SearchAsync_NoMatches_ReturnsEmpty() {
        // Arrange
        var chatId = await SeedChatAsync();
        await _repo.SaveForChatAsync(NewMessage(chatId, "Nothing matches here"));

        // Act
        var result = await _repo.SearchAsync(chatId, "unrelated", limit: 10, offset: 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_OrdersByCreatedAtDescending() {
        // Arrange
        var chatId = await SeedChatAsync();
        var oldest = NewMessage(chatId, "Hello oldest", DateTimeOffset.UtcNow.AddMinutes(-10));
        var newest = NewMessage(chatId, "Hello newest", DateTimeOffset.UtcNow);

        await _repo.SaveForChatAsync(oldest);
        await _repo.SaveForChatAsync(newest);

        // Act
        var result = await _repo.SearchAsync(chatId, "Hello", limit: 10, offset: 0);

        // Assert
        Assert.Equal("Hello newest", result[0].PlainText);
        Assert.Equal("Hello oldest", result[1].PlainText);
    }

    [Fact]
    public async Task SearchAsync_PaginationWorks() {
        // Arrange
        var chatId = await SeedChatAsync();
        const int searchLimit = 3;

        for (var i = 0; i < 6; i++) {
            await _repo.SaveForChatAsync(NewMessage(
                chatId, $"Hello {i}",
                DateTimeOffset.UtcNow.AddSeconds(i))
            );
        }

        // Act
        var page1 = await _repo.SearchAsync(chatId, "Hello",
            limit: searchLimit, offset: 0);

        var page2 = await _repo.SearchAsync(chatId, "Hello",
            limit: searchLimit, offset: searchLimit);

        // Assert
        Assert.Equal(searchLimit, page1.Count);
        Assert.Equal(searchLimit, page2.Count);

        var pagesIntersection = page1.Select(m => m.Id)
            .Intersect(page2.Select(m => m.Id));

        Assert.Empty(pagesIntersection);
    }


    [Fact]
    public async Task MarkAsReadAsync_UnreadMessage_MarksAsRead() {
        // Arrange
        var chatId = await SeedChatAsync();
        var message = NewMessage(chatId, isRead: false);
        await _repo.SaveForChatAsync(message);

        // Act
        await _repo.MarkAsReadAsync(message.Id);

        ClearTracker();
        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);

        // Assert
        Assert.True(result[0].IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_AlreadyReadMessage_RemainsRead() {
        // Arrange
        var chatId = await SeedChatAsync();
        var message = NewMessage(chatId, isRead: true);
        await _repo.SaveForChatAsync(message);

        // Act
        await _repo.MarkAsReadAsync(message.Id);
        
        ClearTracker();
        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);

        // Assert
        Assert.True(result[0].IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_NonExistingMessage_DoesNotThrow() {
        // Arrange & Act & Assert
        await _repo.MarkAsReadAsync(IdFactory.Generate<MessageId>());
    }

    [Fact]
    public async Task MarkAsReadAsync_OtherMessagesUnaffected() {
        // Arrange
        var chatId = await SeedChatAsync();

        var toRead = NewMessage(chatId, isRead: false);
        var toKeep = NewMessage(chatId, isRead: false);

        await _repo.SaveForChatAsync(toRead);
        await _repo.SaveForChatAsync(toKeep);

        // Act
        await _repo.MarkAsReadAsync(toRead.Id);
        
        ClearTracker();

        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);
        var updated = result.First(m => m.Id.Equals(toRead.Id));
        var untouched = result.First(m => m.Id.Equals(toKeep.Id));

        // Assert
        Assert.True(updated.IsRead);
        Assert.False(untouched.IsRead);
    }


    [Fact]
    public async Task DeleteAsync_ExistingMessage_RemovesFromDatabase() {
        // Arrange
        var chatId = await SeedChatAsync();
        var message = NewMessage(chatId);

        await _repo.SaveForChatAsync(message);

        // Act
        await _repo.DeleteAsync(message.Id);
        ClearTracker();

        // Assert
        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingMessage_DoesNotThrow() {
        // Arrange & Act & Assert
        await _repo.DeleteAsync(IdFactory.Generate<MessageId>());
    }

    [Fact]
    public async Task DeleteAsync_OtherMessagesUnaffected() {
        // Arrange
        var chatId = await SeedChatAsync();

        var toDelete = NewMessage(chatId, "DELETE ME");
        var toKeep = NewMessage(chatId, "KEEP ME");

        await _repo.SaveForChatAsync(toDelete);
        await _repo.SaveForChatAsync(toKeep);

        // Act
        await _repo.DeleteAsync(toDelete.Id);

        ClearTracker();

        var result = await _repo.FindByChatIdAsync(chatId, limit: 10, offset: 0);

        // Assert
        Assert.Single(result);
        Assert.Equal("KEEP ME", result[0].PlainText);
    }
}