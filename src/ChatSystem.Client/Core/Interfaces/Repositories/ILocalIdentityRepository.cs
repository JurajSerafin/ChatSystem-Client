using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Identity;

namespace ChatSystem.Client.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for securely managing the active user's session identity.
///
/// Handles the persistence of the current logged-in user profile and their
/// active network session token.
/// </summary>
internal interface ILocalIdentityRepository {
    /// <summary>
    /// Saves the active user's identity and session to the local cache.
    /// </summary>
    /// <param name="identity">The identity object to persist.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the identity's store operation.</returns>
    public Task StoreAsync(CachedIdentity identity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the currently logged-in user's identity.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting CachedIdentity instance containing the identity if an active session exists, null otherwise.</returns>
    public Task<CachedIdentity?> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the active session token without modifying the rest of the user's identity.
    /// </summary>
    /// <param name="newToken">The new network session token.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the session-updating operation.</returns>
    public Task UpdateSessionTokenAsync(string newToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Wipes the active identity and session token from the local cache.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the cache-clearing operation.</returns>
    public Task ClearAsync(CancellationToken cancellationToken = default);
}