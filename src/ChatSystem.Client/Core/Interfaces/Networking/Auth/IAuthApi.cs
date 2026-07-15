using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Interfaces.Networking.Auth.DTOs;
using Refit;

namespace ChatSystem.Client.Core.Interfaces.Networking.Auth;

/// <summary>
/// Defines the API client contract for authentication server requests.
/// This interface is utilized by Refit to generate network request implementations at runtime.
/// </summary>
internal interface IAuthApi {

    /// <summary>
    /// Registers a new user account with the server.
    /// </summary>
    /// <param name="registerRequest">The payload containing registration details.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous registration operation, containing the <see cref="AuthResponse"/> upon success.</returns>
    [Post("/auth/register")]
    Task<AuthResponse> RegisterAsync([Body] RegisterRequest registerRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates an existing user using their login credentials.
    /// </summary>
    /// <param name="loginRequest">The payload containing the user's identifier and password.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous login operation, containing the <see cref="AuthResponse"/> upon success.</returns>
    [Post("/auth/login")]
    Task<AuthResponse> LoginAsync([Body] LoginRequest loginRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests the server to invalidate the provided session token, logging the user out.
    /// </summary>
    /// <param name="logoutRequest">The payload containing the session token to be terminated.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous logout operation.</returns>
    [Post("/auth/logout")]
    Task LogoutAsync([Body] LogoutRequest logoutRequest, CancellationToken cancellationToken = default);
}