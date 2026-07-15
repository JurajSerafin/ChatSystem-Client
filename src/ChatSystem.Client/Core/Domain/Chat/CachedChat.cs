using System;

namespace ChatSystem.Client.Core.Domain.Chat;


/// <summary>
/// Represents a chat room's metadata cached in the local database.
/// </summary>
internal class CachedChat {

    /// <summary>
    /// The unique identifier of the chat room.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The ID of the most recent message in the chat, if any exist.
    /// </summary>
    public string? LastMessageId { get; set; }

    /// <summary>
    /// The assigned name of the chat room (typically null for 1-on-1 chats).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Timestamp of when this chat was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Timestamp of the last activity (message sent/received) in this chat.
    /// </summary>
    public required DateTimeOffset LastActivityAt { get; set; }

    /// <summary>
    /// Timestamp of when this chat metadata was last synced.
    /// </summary>
    public required DateTimeOffset CachedAt { get; set; }

    /// <summary>
    /// Flag indicating if the chat has been soft-deleted locally.
    /// </summary>
    public required bool IsDeleted { get; set; } = false;

}