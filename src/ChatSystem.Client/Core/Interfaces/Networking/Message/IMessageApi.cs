using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Interfaces.Networking.Message.DTOs;
using Refit;

namespace ChatSystem.Client.Core.Interfaces.Networking.Message;

/// <summary>
/// Defines the API client contract for message-related server requests.
/// This interface is utilized by Refit to generate network request implementations at runtime.
/// </summary>
internal interface IMessageApi {

    /// <summary>
    /// Sends an encrypted text message to a specific chat room.
    /// </summary>
    /// <param name="id">The destination chat room ID.</param>
    /// <param name="request">The payload containing metadata about message, it's sender, recipient, etc.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A payload carrying the sent encrypted message and its metadata.</returns>
    [Post("/chats/{id}/messages")]
    Task<SingleMessageResponse> SendMessageAsync(ChatId id, [Body] SendMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated history of fully decrypted messages for a chat.
    /// </summary>
    /// <param name="id">The target chat room ID.</param>
    /// <param name="limit">The maximum number of messages to fetch.</param>
    /// <param name="offset">The pagination offset.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A collection of payloads carrying encrypted messages and their metadata.</returns>
    [Get("/chats/{id}/messages")]
    Task<IReadOnlyList<SingleMessageResponse>> GetHistoryAsync(
        ChatId id,
        [Query] int limit,
        [Query] int offset,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Marks a message as read, updating the server state.
    /// </summary>
    /// <param name="id">The unique identifier of the message.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous read-state marking operation.</returns>
    [Put("/messages/{id}/read")]
    Task MarkAsReadAsync(MessageId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the maximum number of messages delivered while the user was offline.
    /// </summary>
    /// <param name="limit">The maximum number of messages to fetch.</param>
    /// <param name="afterId">The offset message, after which the fetching should start.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A collection of payloads carrying encrypted undelivered messages and their metadata.</returns>
    [Get("/messages/undelivered")]
    Task<IReadOnlyList<SingleMessageResponse>> GetUndeliveredAsync(
        [Query] int limit,
        [Query] MessageId? afterId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the encrypted symmetric key for a specific message,
    /// wrapped with the calling user's public key.
    /// </summary>
    [Get("/messages/{id}/key")]
    Task<GetEncryptedKeyResponse> GetEncryptedKeyAsync(
        MessageId id,
        CancellationToken cancellationToken = default);
}