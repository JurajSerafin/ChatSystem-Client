using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;

namespace ChatSystem.Client.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing cached messages within chat rooms.
///
/// Provides methods for querying, storing, and updating the state of fully
/// decrypted messages stored in the local SQLite database.
/// </summary>
internal interface ILocalMessageRepository {

    /// <summary>
    /// Retrieves a paginated history of messages for a specific chat room.
    /// </summary>
    /// <param name="chatId">The ID of the chat room.</param>
    /// <param name="limit">The maximum number of messages to retrieve.</param>
    /// <param name="offset">The pagination offset.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting a list of decrypted messages.</returns>
    public Task<IReadOnlyList<CachedMessage>> FindByChatIdAsync(ChatId chatId, int limit, int offset, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a local plaintext search for messages matching specific keywords.
    /// </summary>
    /// <param name="chatId"> The ID of the chat room to search within.</param>
    /// <param name="keywords">The search query string.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting a list of messages matching the search criteria.</returns>
    public Task<IReadOnlyList<CachedMessage>> SearchAsync(ChatId chatId, string keywords, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific message entirely from the local cache.
    /// </summary>
    /// <param name="id">The ID of the message to delete.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the message-deleting operation.</returns>
    public Task DeleteAsync(MessageId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the read status of a locally cached message.
    /// </summary>
    /// <param name="messageId">The ID of the message to mark as read.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the read-marking operation of the specified message.</returns>
    public Task MarkAsReadAsync(MessageId messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a newly received or sent message specifically tied to its chat.
    /// </summary>
    /// <param name="message">The message object to append to the chat history.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the chet-saving operation.</returns>
    public Task SaveForChatAsync(CachedMessage message, CancellationToken cancellationToken = default);
}