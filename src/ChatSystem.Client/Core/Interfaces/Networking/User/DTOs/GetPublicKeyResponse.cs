using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Interfaces.Networking.User.DTOs;


/// <summary>
/// Represents the client's request to fetch specified user's public key the server.
/// </summary>
/// <param name="PublicKey">The user's public key string.</param>
internal record GetPublicKeyResponse (
    [property: JsonPropertyName("public_key")]
    string PublicKey
);