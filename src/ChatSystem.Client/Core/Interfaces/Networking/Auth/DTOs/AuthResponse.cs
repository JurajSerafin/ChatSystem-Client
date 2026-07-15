using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Interfaces.Networking.Auth.DTOs;

/// <summary>
/// Represents the server response payload received upon a successful authentication event. (see: <see cref="IAuthApi"></see>)/>.
/// </summary>
/// <param name="SessionToken">The active cryptographic session token used to authorize subsequent requests.</param>
/// <param name="Id">The unique identity string associated with the authenticated user.</param>
/// <param name="Login">The user's login.</param>
/// <param name="Tag">The unique suffix tag assigned to the user profile.</param>
internal record AuthResponse(
    [property: JsonPropertyName("token")] string SessionToken,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("tag")] string Tag
);