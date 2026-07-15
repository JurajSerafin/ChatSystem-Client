using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;

namespace ChatSystem.Client.Core.Interfaces.Networking.Message;

/// <summary>
/// Interface for a service responsible for the End-to-End Encrypted (E2EE) messaging pipeline.
///
/// Handles generating payload keys, encrypting outgoing plaintext, decrypting incoming ciphertext
/// and persisting plaintext history in the local cache.
/// </summary>
internal interface IMessageService {

    /// <summary>
    /// Encrypts and dispatches a text message to a specific chat room.
    ///
    /// Generates a symmetric key, encrypts the text, wraps the key for each participant
    /// and posts the payload to the server.
    /// </summary>
    /// <param name="chatId">The destination chat room ID.</param>
    /// <param name="plainText">The unencrypted message text.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting a CachedMessage instance representing the sent message.</returns>
    public Task<CachedMessage> SendMessageAsync(ChatId chatId, string plainText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated history of fully decrypted messages for a chat.
    /// </summary>
    /// <param name="chatId">The target chat room ID.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task awaiting a list of decrypted CachedMessage objects.</returns>
    public Task<IReadOnlyList<CachedMessage>> GetHistoryAsync(ChatId chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as read, updating both the server state and the local cache.
    /// </summary>
    /// <param name="messageId">he unique identifier of the message.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>>A task representing the asynchronous read-state marking operation.</returns>
    public Task MarkAsReadAsync(MessageId messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches, decrypts, and caches the maximum number of messages delivered while the user was offline.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>A task awaiting a list of newly decrypted CachedMessage objects.</returns>
    public Task<IReadOnlyList<CachedMessage>> GetUndeliveredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a local plaintext search across a chat's history.
    ///
    /// Search is executed strictly on the client-side local cache to maintain E2EE security.
    /// </summary>
    /// <param name="chatId">The ID of the chat room to search within.</param>
    /// <param name="keyWords">The search query string.</param>
    /// <returns>A task awaiting a list of messages matching the criteria.</returns>
    public Task<IReadOnlyList<CachedMessage>> SearchMessagesAsync(ChatId chatId, string keyWords);
}