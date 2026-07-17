using System;
using System.Collections.Generic;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Ids;

namespace ChatSystem.Client.Tests.Core.User;

public class UserIdTests {
    [Fact]
    public void Create_WithValidGuid_ReturnsExpectedValue() {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var userId = UserId.Create(guid);

        // Assert
        Assert.Equal(guid, userId.Value);
    }

    [Fact]
    public void Generate_ReturnsUserIdWithNonEmptyGuid() {
        // Act
        var userId = IdFactory.Generate<UserId>();

        // Assert
        Assert.NotEqual(Guid.Empty, userId.Value);
    }

    [Fact]
    public void Generate_ReturnsUniqueValues() {
        // Act
        var id1 = IdFactory.Generate<UserId>();
        var id2 = IdFactory.Generate<UserId>();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Equals_SameGuid_ReturnsTrue() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = UserId.Create(guid);
        var id2 = UserId.Create(guid);

        // Assert
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Equals_DifferentGuid_ReturnsFalse() {
        // Arrange
        var id1 = IdFactory.Generate<UserId>();
        var id2 = IdFactory.Generate<UserId>();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void GetHashCode_SameGuid_ReturnsSameHash() {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = UserId.Create(guid);
        var id2 = UserId.Create(guid);

        // Assert
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void CanBeUsedAsDictionaryKey() {
        // Arrange
        var id = IdFactory.Generate<UserId>();
        var dictionary = new Dictionary<UserId, string> {
            [id] = "test-value"
        };

        // Assert
        Assert.Equal("test-value", dictionary[id]);
    }
}