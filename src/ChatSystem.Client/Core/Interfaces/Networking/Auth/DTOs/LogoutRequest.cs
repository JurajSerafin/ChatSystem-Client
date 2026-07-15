using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Interfaces.Networking.Auth.DTOs;

/// <summary>
/// Represents the client request payload used to terminate an active user session.
/// </summary>
/// <param name="SessionToken">The active session token that the server should invalidate.</param>
internal record LogoutRequest(
    [property: JsonPropertyName("token")] string SessionToken
);