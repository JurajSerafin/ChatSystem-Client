using System.Collections.Generic;
using System.Text.Json.Serialization;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Networking.Message.DTOs;

/// <summary>
/// Represents the client request send a message to the server.
/// </summary>
/// <param name="CipherText">The encrypted message.</param>
/// <param name="EncryptedKeys">The encrypted public keys of the message's recipients.</param>
/// <param name="MessageType">The type descriptor of the message (e.g., "TEXT", "IMAGE", etc.).</param>
internal record SendMessageRequest(
    [property: JsonPropertyName("ciphertext")]
    string CipherText,

    [property: JsonPropertyName("encrypted_keys")]
    IReadOnlyDictionary<UserId, string> EncryptedKeys,

    [property: JsonPropertyName("type")]
    string MessageType
);