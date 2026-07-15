using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Networking.User.DTOs;
using Refit;

namespace ChatSystem.Client.Core.Interfaces.Networking.User;


/// <summary>
/// Defines the API client contract for user-related server requests.
/// This interface is utilized by Refit to generate network request implementations at runtime.
/// </summary>
internal interface IUserApi {

    /// <summary>
    /// Searches the global user directory on the server for matching accounts.
    /// </summary>
    /// <param name="q">The search string (can be a login or tag).</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="offset">The pagination offset.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A collection of matching user profiles.</returns>
    [Get("/users/search")]
    Task<IReadOnlyList<SingleUserResponse>> SearchUsersAsync(
        [Query] string q,
        [Query] int limit,
        [Query] int offset,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a specific user profile.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The payload carrying metadata about found profile.</returns>
    [Get("/users/{id}")]
    Task<SingleUserResponse> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the public key of a specific user.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The user's public key string.</returns>
    [Get("/users/{id}/public-key")]
    Task<GetPublicKeyResponse> GetPublicKeyAsync(UserId id, CancellationToken cancellationToken = default);
}