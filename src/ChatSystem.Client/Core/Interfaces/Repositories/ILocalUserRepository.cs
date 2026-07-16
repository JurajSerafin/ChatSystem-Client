using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing cached user profiles.
///
/// Handles the persistence of public user profiles and their public keys,
/// allowing the client to look up participants without hitting the network.
/// </summary>
internal interface ILocalUserRepository {

    /// <summary>
    /// Retrieves a specific user profile by their unique ID.
    /// </summary>
    /// <param name="userId">The ID of the user to locate.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task awaiting a <see cref="CachedUser"/> instance containing the user profile if found locally, null otherwise.</returns>
    public Task<CachedUser?> FindByIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user profile by either an exact login username or a display tag.
    /// </summary>
    /// <param name="searchVal">The login or tag to search for.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting a <see cref="CachedUser"/> instance containing the user profile if found locally, null otherwise.</returns>
    public Task<CachedUser?> FindByLoginOrTagAsync(string searchVal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new user profile or updates an existing one in the local cache.
    /// </summary>
    /// <param name="user">The user profile object to save.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the user-upserting operation.</returns>
    public Task UpsertAsync(CachedUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user profile from the local cache.
    /// </summary>
    /// <param name="userId">The ID of the user to delete.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the user-deleting operation.</returns>
    public Task DeleteAsync(UserId userId, CancellationToken cancellationToken = default);

}