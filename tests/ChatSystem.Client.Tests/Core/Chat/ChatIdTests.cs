using System;
using System.Collections.Generic;
using System.Linq;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Interfaces.Ids;

namespace ChatSystem.Client.Tests.Core.Chat;

public class ChatIdTests {
    [Fact]
    public void Create_WithValidGuid_ReturnsExpectedValue() {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var chatId = ChatId.Create(guid);

        // Assert
        Assert.Equal(guid, chatId.Value);
    }

    [Fact]
    public void Generate_ReturnsChatIdWithNonEmptyGuid() {
        // Act
        var chatId = IdFactory.Generate<ChatId>();

        // Assert
        Assert.NotEqual(Guid.Empty, chatId.Value);
    }

    [Fact]
    public void Generate_ReturnsUniqueValues() {
        // Act
        var id1 = IdFactory.Generate<ChatId>();
        var id2 = IdFactory.Generate<ChatId>();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Equals_SameGuid_ReturnsTrue() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ChatId.Create(guid);
        var id2 = ChatId.Create(guid);

        // Assert
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Equals_DifferentGuid_ReturnsFalse() {
        // Arrange
        var id1 = IdFactory.Generate<ChatId>();
        var id2 = IdFactory.Generate<ChatId>();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void EqualityOperator_SameGuid_ReturnsTrue() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ChatId.Create(guid);
        var id2 = ChatId.Create(guid);

        // Assert
        Assert.True(id1 == id2);
    }

    [Fact]
    public void InequalityOperator_DifferentGuid_ReturnsTrue() {
        // Arrange
        var id1 = IdFactory.Generate<ChatId>();
        var id2 = IdFactory.Generate<ChatId>();

        // Assert
        Assert.True(id1 != id2);
    }

    [Fact]
    public void GetHashCode_SameGuid_ReturnsSameHash() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ChatId.Create(guid);
        var id2 = ChatId.Create(guid);

        // Assert
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentGuid_ReturnsDifferentHash() {
        // Arrange
        var id1 = IdFactory.Generate<ChatId>();
        var id2 = IdFactory.Generate<ChatId>();

        // Assert
        Assert.NotEqual(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void CanBeUsedAsDictionaryKey() {
        // Arrange
        var id = IdFactory.Generate<ChatId>();
        var dictionary = new Dictionary<ChatId, string> {
            [id] = "test-value"
        };

        // Assert
        Assert.Equal("test-value", dictionary[id]);
    }

    [Fact]
    public void UsedAsDictionaryKey_SeparateInstanceWithSameGuid_ResolvesCorrectly() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ChatId.Create(guid);
        var id2 = ChatId.Create(guid);

        var dictionary = new Dictionary<ChatId, string> {
            [id1] = "test-value"
        };

        // Assert — different instance, same value, must resolve
        Assert.Equal("test-value", dictionary[id2]);
    }

    [Fact]
    public void Generate_ProducesUniqueIds_InBulk() {
        // Arrange
        var ids = Enumerable
            .Range(0, 1000)
            .Select(_ => IdFactory.Generate<ChatId>())
            .ToList();

        // Assert
        Assert.Equal(1000, ids.Distinct().Count());
    }
}