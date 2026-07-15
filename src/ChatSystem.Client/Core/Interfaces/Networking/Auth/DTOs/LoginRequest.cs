using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Interfaces.Networking.Auth.DTOs;

/// <summary>
/// Represents the client request payload containing user credentials required to log in.
/// </summary>
/// <param name="Login">The user's login.</param>
/// <param name="Password">The plain-text password to be validated by the authentication server.</param>
internal record LoginRequest(
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("password")] string Password
);

