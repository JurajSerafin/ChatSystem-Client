using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Services;

/// <summary>
/// Interface for a service orchestrating user authentication and local session management.
///
/// Coordinates network requests, cryptographic key generation/unlocking, and local cache database updates
/// during the login and registration flows.
/// </summary>
internal interface IAuthService {

    /// <summary>
    /// Registers a new user, generates their cryptographic key pair, and establishes a session.
    /// </summary>
    /// <param name="login">The username.</param>
    /// <param name="password">The plaintext password (used to protect the locally generated private key).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting a CachedUser profile representing the newly registered account.</returns>
    public Task<CachedUser> RegisterAsync(string login, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates an existing user, unlocks their private key, and stores the session token.
    /// </summary>
    /// <param name="login">The username.</param>
    /// <param name="password">The plaintext password (used to protect the locally generated private key).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting a CachedUser profile representing the logged-in account.</returns>
    public Task<CachedUser> LoginAsync(string login, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminates the active session and safely cleanses sensitive data.
    ///
    /// Notifies the server of the logout, drops the session token, and securely wipes
    /// the active private key from memory.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous logout operation.</returns>
    public Task LogoutAsync(CancellationToken cancellationToken = default);
}