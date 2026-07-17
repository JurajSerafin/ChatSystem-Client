using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Networking.Chat;
using ChatSystem.Client.Core.Interfaces.Networking.Chat.DTOs;
using ChatSystem.Client.Core.Interfaces.Repositories;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Infrastructure.Networking.ResponseMappers.Chat;
using ChatSystem.Client.Infrastructure.Networking.ResponseMappers.User;

namespace ChatSystem.Client.Infrastructure.Services;

internal sealed class ChatService : IChatService {
    private const int ChatPageLimit = 50;

    private readonly IChatApi _chatApi;
    private readonly ILocalChatRepository _chatRepo;
    private readonly ILocalUserRepository _userRepo;

    public ChatService(
        IChatApi chatApi,
        ILocalChatRepository chatRepo,
        ILocalUserRepository userRepo) {
        _chatApi = chatApi;
        _chatRepo = chatRepo;
        _userRepo = userRepo;
    }

    public async Task<IReadOnlyList<CachedChat>> GetChatsAsync(
        CancellationToken cancellationToken = default) {

        var response = await _chatApi.GetChatsAsync(
            ChatPageLimit, 0, cancellationToken
        );

        var chats = response.Select(ChatMapper.ToCachedChat).ToList();

        foreach (var chat in chats) {
            await _chatRepo.UpsertAsync(chat, cancellationToken);
        }

        return chats;
    }

    public async Task<CachedChat?> GetChatByIdAsync(
        ChatId id,
        CancellationToken cancellationToken = default)
    {
        var cached = await _chatRepo.FindByIdAsync(id, cancellationToken);

        if (cached is not null) {
            return cached;
        }

        try {
            var response = await _chatApi.GetChatByIdAsync(id, cancellationToken);
            var chat = ChatMapper.ToCachedChat(response);
            await _chatRepo.UpsertAsync(chat, cancellationToken);
            return chat;
        } catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
            return null;
        }
    }

    public async Task<CachedChat> CreateChatAsync(
        IEnumerable<UserId> participants,
        CancellationToken cancellationToken = default)
    {
        var participantIds = participants.ToList();

        var response = await _chatApi.CreateChatAsync(
            new CreateChatRequest(participantIds),
            cancellationToken
        );

        var chat = ChatMapper.ToCachedChat(response);

        await _chatRepo.UpsertAsync(chat, cancellationToken);

        return chat;
    }

    public async Task<IReadOnlyList<CachedUser>> GetParticipantsAsync(
        ChatId chatId,
        CancellationToken cancellationToken = default
    ) {
        var cachedIds = await _chatRepo.GetParticipantIdsAsync(chatId, cancellationToken);

        if (cachedIds.Count > 0) {
            var cachedUsers = new List<CachedUser>();

            foreach (var userId in cachedIds) {
                var user = await _userRepo.FindByIdAsync(userId, cancellationToken);

                if (user is not null) {
                    cachedUsers.Add(user);
                }
            }
            if (cachedUsers.Count == cachedIds.Count) {
                return cachedUsers;
            }
        }

        var response = await _chatApi.GetParticipantsAsync(chatId, cancellationToken);
        var users = response.Select(UserMapper.ToCachedUser).ToList();

        const string defaultRole = "member";

        foreach (var user in users) {
            await _userRepo.UpsertAsync(user, cancellationToken);
            await _chatRepo.AddParticipantAsync(user.Id, chatId, defaultRole, cancellationToken);
        }

        return users;
    }
}