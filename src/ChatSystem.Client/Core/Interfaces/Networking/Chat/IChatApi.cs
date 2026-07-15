using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Networking.Chat.DTOs;
using ChatSystem.Client.Core.Interfaces.Networking.User;
using Refit;

namespace ChatSystem.Client.Core.Interfaces.Networking.Chat;

/// <summary>
/// Defines the API client contract for chat-related operations.
/// This interface is utilized by Refit to generate network request implementations at runtime.
/// </summary>
internal interface IChatApi {

    /// <summary>
    /// Retrieves a paginated list of chat rooms the active user belongs to.
    /// </summary>
    /// <param name="limit">The maximum number of chats to fetch.</param>
    /// <param name="offset">The pagination offset.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The list of JSON payloads representing metadata for individual returned chats.</returns>
    [Get("/chats?limit={}&offset{}")]
    Task<IReadOnlyList<SingleChatResponse>> GetChatsAsync(int limit, int offset,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves details for a specific chat room.
    /// </summary>
    /// <param name="chadId">The unique identifier of the chat room.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The JSON payload representing metadata for the returned chat.</returns>
    [Get("/chats/{id}")]
    Task<SingleChatResponse> GetChatByIdAsync(ChatId chadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new chat room on the server.
    /// </summary>
    /// <param name="participants">The collection of user IDs to include in the newly created chat.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The JSON payload representing metadata for the returned chat.</returns>
    [Post("/chats")]
    Task<SingleChatResponse> CreateChatAsync(ICollection<UserId> participants,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the list of users currently participating in a specific chat room.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat room.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The JSON payload representing metadata for the returned chat.</returns>
    [Get("/chats/{id}/participants")]
    Task<IReadOnlyList<SingleUserResponse>> GetParticipantsAsync(ChatId chatId, CancellationToken cancellationToken = default);


}
