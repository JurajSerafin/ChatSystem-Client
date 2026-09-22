using System;
using System.Linq;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatSystem.Client.Presentation.ViewModels;

/// <summary>
/// ViewModel representing an individual chat entry within the sidebar list.
///
/// Encapsulates row-level UI state and manages display name resolution 
/// for 1-on-1 chats where a chat name is not explicitly defined.
/// </summary>
internal partial class DirectMessageChatListItemViewModel : ViewModelBase {
    private readonly IChatService _chatService;

    private readonly ISessionContext _sessionContext;

    public CachedChat Chat { get; }

    [ObservableProperty] private string _displayName = "Loading...";

    public DirectMessageChatListItemViewModel(
        CachedChat chat,
        IChatService chatService,
        ISessionContext sessionContext
    ) {
        Chat = chat;
        _chatService = chatService;
        _sessionContext = sessionContext;

        if (!string.IsNullOrWhiteSpace(chat.Name)) {
            DisplayName = chat.Name;
        }
    }

    /// <summary>
    /// Fetches chat participants to derive and set the display name 
    /// for direct messages where <see cref="CachedChat.Name"/> is null or empty.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public async Task InitializeAsync() {
        if (!string.IsNullOrWhiteSpace(Chat.Name)) {
            return;
        }

        try {
            var participants = await _chatService.GetParticipantsAsync(Chat.Id);

            var otherUser = participants.FirstOrDefault(participant => !participant.Id.Equals(_sessionContext.CurrentUserId));

            FromChatParticipantDeriveDisplayName(otherUser);

        } catch (Exception ex) {
            FromExDeriveDisplayName(ex);
        }
    }

    private void FromChatParticipantDeriveDisplayName(CachedUser? otherUser) {
        DisplayName = otherUser is not null
            ? otherUser.Login
            : "[Empty Participant List]";
    }

    private void FromExDeriveDisplayName(Exception ex) {
        DisplayName = ex is Refit.ApiException refitEx
            ? FromRefitExDeriveDisplayErrMessage(refitEx)
            : $"Err: {ex.GetType().Name}";
    }

    private static string FromRefitExDeriveDisplayErrMessage(Refit.ApiException apiEx) {
        return $"Err: {(int)apiEx.StatusCode} {apiEx.StatusCode}";
    }
}
