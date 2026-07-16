using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Interfaces.Ids;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatSystem.Client.Tests.Core.IdTests;

public class MessageIdTests {
    [Fact]
    public void Create_WithValidGuid_ReturnsExpectedValue() {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var messageId = MessageId.Create(guid);

        // Assert
        Assert.Equal(guid, messageId.Value);
    }

    [Fact]
    public void Create_ReturnsNonDefaultValue() {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var messageId = MessageId.Create(guid);

        // Assert
        Assert.NotEqual(default, messageId);
    }

    [Fact]
    public void Generate_ReturnsNonDefaultValue() {
        // Act
        var messageId = IdFactory.Generate<MessageId>();

        // Assert
        Assert.NotEqual(default, messageId);
    }

    [Fact]
    public void Generate_ReturnsMessageIdWithNonEmptyGuid() {
        // Act
        var messageId = IdFactory.Generate<MessageId>();

        // Assert
        Assert.NotEqual(Guid.Empty, messageId.Value);
    }

    [Fact]
    public void Generate_ReturnsUniqueValues() {
        // Act
        var id1 = IdFactory.Generate<MessageId>();
        var id2 = IdFactory.Generate<MessageId>();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Equals_SameGuid_ReturnsTrue() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = MessageId.Create(guid);
        var id2 = MessageId.Create(guid);

        // Assert
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Equals_DifferentGuid_ReturnsFalse() {
        // Arrange
        var id1 = IdFactory.Generate<MessageId>();
        var id2 = IdFactory.Generate<MessageId>();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void EqualityOperator_SameGuid_ReturnsTrue() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = MessageId.Create(guid);
        var id2 = MessageId.Create(guid);

        // Assert
        Assert.True(id1 == id2);
    }

    [Fact]
    public void InequalityOperator_DifferentGuid_ReturnsTrue() {
        // Arrange
        var id1 = IdFactory.Generate<MessageId>();
        var id2 = IdFactory.Generate<MessageId>();

        // Assert
        Assert.True(id1 != id2);
    }

    [Fact]
    public void GetHashCode_SameGuid_ReturnsSameHash() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = MessageId.Create(guid);
        var id2 = MessageId.Create(guid);

        // Assert
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentGuid_ReturnsDifferentHash() {
        // Arrange
        var id1 = IdFactory.Generate<MessageId>();
        var id2 = IdFactory.Generate<MessageId>();

        // Assert
        Assert.NotEqual(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void CanBeUsedAsDictionaryKey() {
        // Arrange
        var id = IdFactory.Generate<MessageId>();
        var dictionary = new Dictionary<MessageId, string> {
            [id] = "test-value"
        };

        // Assert
        Assert.Equal("test-value", dictionary[id]);
    }

    [Fact]
    public void UsedAsDictionaryKey_SeparateInstanceWithSameGuid_ResolvesCorrectly() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = MessageId.Create(guid);
        var id2 = MessageId.Create(guid);

        var dictionary = new Dictionary<MessageId, string> {
            [id1] = "test-value"
        };

        // Assert
        Assert.Equal("test-value", dictionary[id2]);
    }

    [Fact]
    public void Generate_ProducesUniqueIds_InBulk() {
        // Arrange
        var ids = Enumerable
            .Range(0, 1000)
            .Select(_ => IdFactory.Generate<MessageId>())
            .ToList();

        // Assert
        Assert.Equal(1000, ids.Distinct().Count());
    }
}