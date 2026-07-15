using System.Collections.Generic;
using System.Text.Json.Serialization;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Interfaces.Networking.Chat.DTOs;

/// <summary>
/// Represents the client request create a new chat room on the server.
/// </summary>
/// <param name="ParticipantIds">The collection of user IDs to include in the newly created chat.</param>
internal record CreateChatRequest(
    [property: JsonPropertyName("participant_ids")] IReadOnlyList<UserId> ParticipantIds
);