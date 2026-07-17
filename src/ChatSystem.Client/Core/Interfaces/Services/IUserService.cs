using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Services;

/// <summary>
/// Interface for a service responsible for querying and caching public user profiles.
///
/// Orchestrates network lookups and local caching to ensure the client always has access
/// to the public keys required for message encryption.
/// </summary>
internal interface IUserService {

    /// <summary>
    /// Searches the global user directory on the server for matching accounts.
    /// </summary>
    /// <param name="query">The search string (can be a login or tag).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns></returns>
    public Task<IReadOnlyList<CachedUser>> SearchAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific user profile.
    ///
    /// First checks the local cache. If missing, it fetches the profile from
    /// the server and persists it locally.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting the requested CachedUser profile or null
    /// if no user is found with the queried ID.</returns>
    public Task<CachedUser?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the public key of a specific user.
    ///
    /// Crucial for E2EE wrapping. Automatically fetches and caches the entire user profile
    /// if they do not currently exist in the local database.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The user's public key string.</returns>
    public Task<string> GetPublicKeyAsync(UserId userId, CancellationToken cancellationToken = default);
}