using System;

namespace ChatSystem.Client.Core.Entities;

/// <summary>
/// Represents a fully decrypted message cached in the local database.
/// </summary>
internal class CachedMessage {

    /// <summary>
    /// The unique identifier of the message.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The unique identifier of the user who sent the message.
    /// </summary>
    public required string SenderId { get; set; }

    /// <summary>
    /// The unique identifier of the chat room this message belongs to.
    /// </summary>
    public required string ChatId { get; set; }

    /// <summary>
    /// The decrypted, raw text of the message.
    /// </summary>
    public required string PlainText { get; set; }

    /// <summary>
    /// The type descriptor of the message (e.g., "TEXT", "IMAGE", etc.).
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Timestamp of when the message was originally created on the server.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Flag indicating if the active user has read this message.
    /// </summary>
    public required bool IsRead { get; set; }

    /// <summary>
    /// Flag indicating if the message was successfully delivered.
    /// </summary>
    public required bool IsDelivered { get; set; }

    /// <summary>
    /// Flag indicating if the message was soft-deleted locally.
    /// </summary>
    public required bool IsDeleted { get; set; }
}

