using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Cryptography;
using ChatSystem.Client.Core.Interfaces.Networking.Message;
using ChatSystem.Client.Core.Interfaces.Networking.Message.DTOs;
using ChatSystem.Client.Core.Interfaces.Repositories;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;

namespace ChatSystem.Client.Infrastructure.Services;

internal sealed class MessageService : IMessageService {
    private const int HistoryPageLimit = 100;
    private const int UndeliveredPageLimit = 200;
    private const string MessageType = "TEXT";

    private readonly IMessageApi _messageApi;
    private readonly IClientEncryptionService _crypto;
    private readonly IClientKeyManager _keyManager;
    private readonly IChatService _chatService;
    private readonly IUserService _userService;
    private readonly ILocalMessageRepository _messageRepo;
    private readonly ISessionContext _session;

    public MessageService(
        IMessageApi messageApi,
        IClientEncryptionService crypto,
        IClientKeyManager keyManager,
        IChatService chatService,
        IUserService userService,
        ILocalMessageRepository messageRepo,
        ISessionContext session) {
        _messageApi = messageApi;
        _crypto = crypto;
        _keyManager = keyManager;
        _chatService = chatService;
        _userService = userService;
        _messageRepo = messageRepo;
        _session = session;
    }

    public async Task<CachedMessage> SendMessageAsync(
        ChatId chatId,
        string plainText,
        CancellationToken cancellationToken = default
    ) {
        EnsureAuthenticated();

        var sessionKey = _crypto.GenerateSymmetricKey();

        var ciphertext = _crypto.EncryptSymmetric(plainText, sessionKey);
        var ciphertextBase64 = Convert.ToBase64String(ciphertext);

        var participants = await _chatService.GetParticipantsAsync(chatId, cancellationToken);
        var encryptedKeys = new Dictionary<UserId, string>();

        foreach (var participant in participants) {
            var publicKey = !string.IsNullOrEmpty(participant.PublicKey)
                ? participant.PublicKey
                : await _userService.GetPublicKeyAsync(participant.Id, cancellationToken);

            var wrapped = _crypto.WrapKey(sessionKey, publicKey);
            encryptedKeys[participant.Id] = Convert.ToBase64String(wrapped);
        }

        var response = await _messageApi.SendMessageAsync(
            chatId,
            new SendMessageRequest(ciphertextBase64, encryptedKeys, MessageType),
            cancellationToken);

        var cached = new CachedMessage {
            Id = response.Id,
            ChatId = chatId,
            SenderId = response.SenderId,
            PlainText = plainText,
            Type = MessageType,
            CreatedAt = response.CreatedAt,
            IsRead = false,
            IsDelivered = true,
            IsDeleted = false
        };
        await _messageRepo.SaveForChatAsync(cached, cancellationToken);

        return cached;
    }

    public async Task<IReadOnlyList<CachedMessage>> GetHistoryAsync(
        ChatId chatId,
        CancellationToken cancellationToken = default) {
        EnsureAuthenticated();

        var cached = await _messageRepo.FindByChatIdAsync(
            chatId, HistoryPageLimit,
            0,
            cancellationToken
        );

        if (cached.Count > 0) {
            return cached;
        }

        var response = await _messageApi.GetHistoryAsync(
            chatId,
            HistoryPageLimit,
            0,
            cancellationToken
            );

        var messages = new List<CachedMessage>();
        foreach (var dto in response) {
            var decrypted = await DecryptAndPersistAsync(dto, cancellationToken);

            if (decrypted is not null) {
                messages.Add(decrypted);
            }
        }

        return messages;
    }

    public async Task MarkAsReadAsync(
        MessageId messageId,
        CancellationToken cancellationToken = default
    ) {
        EnsureAuthenticated();

        await _messageApi.MarkAsReadAsync(messageId, cancellationToken);
        await _messageRepo.MarkAsReadAsync(messageId, cancellationToken);
    }

    public async Task<IReadOnlyList<CachedMessage>> GetUndeliveredAsync(
        CancellationToken cancellationToken = default
    ) {
        EnsureAuthenticated();


        var response = await _messageApi.GetUndeliveredAsync(
            UndeliveredPageLimit,
            null,
            cancellationToken
        );

        var messages = new List<CachedMessage>();

        foreach (var dto in response) {
            var decrypted = await DecryptAndPersistAsync(dto, cancellationToken);

            if (decrypted is not null) {
                messages.Add(decrypted);
            }
        }

        return messages;
    }

    public async Task<IReadOnlyList<CachedMessage>> SearchMessagesAsync(
        ChatId chatId,
        string keyWords)
    {
        return await _messageRepo.SearchAsync(chatId, keyWords, HistoryPageLimit, 0);
    }

    private async Task<CachedMessage?> DecryptAndPersistAsync(
        SingleMessageResponse dto,
        CancellationToken cancellationToken
    ) {
        try {
            var keyResponse = await _messageApi.GetEncryptedKeyAsync(dto.Id, cancellationToken);

            var wrappedKey = Convert.FromBase64String(keyResponse.EncryptedKey);
            var sessionKey = _crypto.UnwrapKey(wrappedKey, _keyManager.GetPrivateKey());

            var ciphertext = Convert.FromBase64String(dto.Ciphertext);
            var plaintext = _crypto.DecryptSymmetric(ciphertext, sessionKey);

            var cached = new CachedMessage {
                Id = dto.Id,
                ChatId = dto.ChatId,
                SenderId = dto.SenderId,
                PlainText = plaintext,
                Type = dto.Type,
                CreatedAt = dto.CreatedAt,
                IsRead = false,
                IsDelivered = true,
                IsDeleted = false
            };

            await _messageRepo.SaveForChatAsync(cached, cancellationToken);

            return cached;
        } catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
            return null;
        } catch (Exception) {
            return null;
        }
    }

    private void EnsureAuthenticated() {
        if (!_session.IsAuthenticated) {
            throw new InvalidOperationException("Not authenticated. Log in first.");
        }
    }
}