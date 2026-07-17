using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Services;

/// <summary>
/// Interface for a service responsible for managing chat room entities and their participants.
///
/// Implements the cache-aside pattern to fetch chat data from the server
/// and persist it in the local cache repository for fast UI rendering.
/// </summary>
internal interface IChatService {

    /// <summary>
    /// Retrieves a paginated list of chat rooms the active user belongs to.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>Task awaiting the fetched list of CachedChat DTOs.</returns>
    public Task<IReadOnlyList<CachedChat>> GetChatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves details for a specific chat room.
    /// </summary>
    /// <param name="id">The unique identifier of the chat room.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The task awaiting fetched CachedChat DTO or null if no chat is found
    /// matching the queried ID.</returns>
    public Task<CachedChat?> GetChatByIdAsync(ChatId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new chat room on the server and synchronizes it locally.
    /// </summary>
    /// <param name="participants">User IDs to include in the newly created chat.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The task awaiting created CachedChat DTO.</returns>
    public Task<CachedChat> CreateChatAsync(IEnumerable<UserId> participants, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the list of users currently participating in a specific chat room.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat room.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The task awaiting the fetched list of participants in form of CachedUser DTOs.</returns>
    public Task<IReadOnlyList<CachedUser>> GetParticipantsAsync(ChatId chatId, CancellationToken cancellationToken = default);
}