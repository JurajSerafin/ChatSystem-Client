using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing cached chat rooms and their participants.
///
/// Abstracts the underlying local database to handle chat metadata and
/// the many-to-many junction relationships between users and chats.
/// </summary>
internal interface ILocalChatRepository {

    /// <summary>
    /// Retrieves all cached chat rooms available to the user.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting list of CachedChat objects, typically sorted by recent activity.</returns>
    public Task<IReadOnlyList<CachedChat>> FindAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific chat room by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the chat room to locate.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting the queried CachedChat instance if found, null otherwise.</returns>
    public Task<CachedChat?> FindByIdAsync(ChatId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new chat room or updates an existing one in the local cache.
    /// </summary>
    /// <param name="chat">The chat metadata object to save.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the upsert operation.</returns>
    public Task UpsertAsync(CachedChat chat, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a user to a specific chat room with an assigned role.
    /// </summary>
    /// <param name="userId">The ID of the user to add.</param>
    /// <param name="chatId">The ID of the chat room.</param>
    /// <param name="roleString">The string token representing the assigned role of the user in the chat.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the participant-adding operation.</returns>
    public Task AddParticipantAsync(UserId userId, ChatId chatId, string roleString, CancellationToken cancellationToken = default)

    /// <summary>
    /// Deletes a chat room and cascades deletions to its local messages and participant links.
    /// </summary>
    /// <param name="chatId">The ID of the chat to delete.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the delete operation.</returns>
    public Task DeleteAsync(ChatId chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the IDs of all users participating in a specific chat room.
    /// </summary>
    /// <param name="chatId">The ID of the chat room.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting the list of user ID strings.</returns>
    public IReadOnlyList<ChatId> GetParticipantIdsAsync(ChatId chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user's participation link from a specific chat room.
    /// </summary>
    /// <param name="userId">The ID of the user to remove.</param>
    /// <param name="chatId">The ID of the chat room.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the participant-removing operation.</returns>
    public Task RemoveParticipantAsync(UserId userId, ChatId chatId, CancellationToken cancellationToken = default);
}