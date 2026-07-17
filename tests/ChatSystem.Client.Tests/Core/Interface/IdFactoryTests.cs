using System;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Ids;

namespace ChatSystem.Client.Tests.Core.Interface;

public class IdFactoryTests {
    [Fact]
    public void Generate_ChatId_ReturnsNonDefault() {
        // Act
        var id = IdFactory.Generate<ChatId>();

        // Assert
        Assert.NotEqual(default, id);
    }

    [Fact]
    public void Generate_UserId_ReturnsNonDefault() {
        // Act
        var id = IdFactory.Generate<UserId>();

        // Assert
        Assert.NotEqual(default, id);
    }

    [Fact]
    public void Generate_MessageId_ReturnsNonDefault() {
        // Act
        var id = IdFactory.Generate<MessageId>();

        // Assert
        Assert.NotEqual(default, id);
    }

    [Fact]
    public void Generate_CalledTwice_ReturnsDifferentValues() {
        // Act
        var id1 = IdFactory.Generate<ChatId>();
        var id2 = IdFactory.Generate<ChatId>();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Create_WithGuid_ReturnsIdWithCorrectValue() {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = IdFactory.Create<ChatId>(guid);

        // Assert
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void Create_SameGuid_DifferentTypes_ProducesUnrelatedValues() {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var chatId = IdFactory.Create<ChatId>(guid);
        var userId = IdFactory.Create<UserId>(guid);

        // Assert
        Assert.Equal(guid, chatId.Value);
        Assert.Equal(guid, userId.Value);
    }

    [Fact]
    public void Parse_ValidGuidString_ReturnsIdWithCorrectValue() {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = IdFactory.Parse<ChatId>(guid.ToString());

        // Assert
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void Parse_InvalidString_ThrowsFormatException() {
        Assert.Throws<FormatException>(() => IdFactory.Parse<ChatId>("gibberish"));
    }

    [Fact]
    public void Parse_EmptyString_ThrowsFormatException() {
        Assert.Throws<FormatException>(() => IdFactory.Parse<ChatId>(""));
    }

    [Fact]
    public void Parse_RoundTrip_PreservesValue() {
        // Arrange
        var original = IdFactory.Generate<MessageId>();

        // Act
        var parsed = IdFactory.Parse<MessageId>(original.Value.ToString());

        // Assert
        Assert.Equal(original, parsed);
    }

    [Fact]
    public void Generate_WorksConsistentlyAcrossAllIdTypes() {
        // Arrange & Act
        var chatId = IdFactory.Generate<ChatId>();
        var userId = IdFactory.Generate<UserId>();
        var messageId = IdFactory.Generate<MessageId>();

        // Assert
        Assert.NotEqual(Guid.Empty, chatId.Value);
        Assert.NotEqual(Guid.Empty, userId.Value);
        Assert.NotEqual(Guid.Empty, messageId.Value);
    }
}