using System;
using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Interfaces.Networking.Chat.DTOs;

/// <summary>
/// Represents the server response payload containing metadata of a single chat.
/// </summary>
/// <param name="Id">THe chat's identifier.</param>
/// <param name="CreatedAt">Timestamp of when this chat was created.</param>
/// <param name="LastActivityAt">Timestamp of the last activity (message sent/received) in this chat.</param>
/// <param name="Name">The assigned name of the chat room (typically null for 1-on-1 chats).</param>
/// <param name="LastMessageId">The ID of the most recent message in the chat, if any exist.</param>
internal record SingleChatResponse(
    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("created_at")]
    DateTimeOffset CreatedAt,

    [property: JsonPropertyName("last_activity_at")]
    DateTimeOffset LastActivityAt,

    [property: JsonPropertyName("name")]
    string? Name,

    [property: JsonPropertyName("last_message_id")]
    string? LastMessageId
);
