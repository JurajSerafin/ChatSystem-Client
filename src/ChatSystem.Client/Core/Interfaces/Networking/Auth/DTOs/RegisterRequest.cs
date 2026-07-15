using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Interfaces.Networking.Auth.DTOs;

/// <summary>
/// Represents the client request payload used to establish a new user account, 
/// supplying necessary security credentials.
/// </summary>
/// <param name="Login">The login for the new account.</param>
/// <param name="Password">The plain-text password chosen by the user.</param>
/// <param name="PublicKey">The user's public cryptographic key, utilized for configuring E2EE channels.</param>
internal record RegisterRequest(
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("public_key")] string PublicKey
);