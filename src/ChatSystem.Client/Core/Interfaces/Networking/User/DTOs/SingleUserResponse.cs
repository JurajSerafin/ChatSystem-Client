using System.Text.Json.Serialization;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Networking.User.DTOs;

/// <summary>
/// Represents the server response payload containing metadata of a single user.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Tag">The user's unique display tag.</param>
/// <param name="Login">The user's login.</param>
/// <param name="PublicKey">The user's public key, used to wrap symmetric E2EE keys.</param>
/// <param name="Role">The string token representing a user's role, defining a set of authorized actions. See <see cref="IUserRole"/></param>
internal record SingleUserResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tag")] string Tag,
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("public_key")] string PublicKey,
    [property: JsonPropertyName("role")] string Role
);