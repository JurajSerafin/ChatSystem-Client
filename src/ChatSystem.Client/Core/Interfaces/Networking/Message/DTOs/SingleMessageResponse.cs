using System;
using System.Text.Json.Serialization;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Networking.Message.DTOs {

    /// <summary>
    /// Represents the server response payload containing metadata of a message chat.
    /// </summary>
    /// <param name="Id">The unique identifier of the message.</param>
    /// <param name="SenderId">The unique identifier of the user who sent the message.</param>
    /// <param name="ChatId">The unique identifier of the chat room this message belongs to.</param>
    /// <param name="Ciphertext">The encrypted message.</param>
    /// <param name="Type">The type descriptor of the message (e.g., "TEXT", "IMAGE", etc.).</param>
    /// <param name="CreatedAt">Timestamp of when the message was originally created on the server.</param>
    internal record SingleMessageResponse(
        [property: JsonPropertyName("id")]
        MessageId Id,

        [property: JsonPropertyName("sender_id")]
        UserId SenderId,

        [property: JsonPropertyName("chat_id")]
        ChatId ChatId,

        [property: JsonPropertyName("ciphertext")]
        string Ciphertext,

        [property: JsonPropertyName("type")]
        string Type,

        [property: JsonPropertyName("created_at")]
        DateTimeOffset CreatedAt
    );
}
