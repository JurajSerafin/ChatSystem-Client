using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Interfaces.Networking.Message.DTOs {
    internal record GetEncryptedKeyResponse(
        [property: JsonPropertyName("encrypted_key")] string EncryptedKey
    );
}
