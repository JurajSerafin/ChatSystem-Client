using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatSystem.Client.Presentation.ViewModels;

/// <summary>
/// ViewModel managing the collection of direct message chats displayed in the sidebar navigation pane.
///
/// Handles fetching chats, populating child item ViewModels, 
/// and broadcasting selection changes to parent presentation components.
/// </summary>
internal partial class ChatListViewModel : ViewModelBase {
    private readonly IChatService _chatService;

    private readonly ISessionContext _sessionContext;

    public ObservableCollection<DirectMessageChatListItemViewModel> Chats { get; set; } = [];

    [ObservableProperty] private DirectMessageChatListItemViewModel? _selectedChat;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public event EventHandler<CachedChat?>? ChatSelected;

    public ChatListViewModel(
        IChatService chatService,
        ISessionContext sessionContext
    ) {
        _chatService = chatService;
        _sessionContext = sessionContext;
    }

    /// <summary>
    /// Populates <see cref="Chats"/> initializes individual child item (row) ViewModels.
    /// </summary>
    /// <returns>A task representing the asynchronous chat loading operation.</returns>
    public async Task LoadChatsAsync() {
        IsBusy = true;

        ErrorMessage = null;

        try {
            var chats = await _chatService.GetChatsAsync();

            Chats.Clear();

            foreach (var chat in chats) {
                var chatItemVm = FromCachedChatCreateChatListItemViewModel(chat);

                Chats.Add(chatItemVm);

                await chatItemVm.InitializeAsync();
            }
        } catch (Exception ex) {
            ErrorMessage = $"Failed to load chats: {ex.Message}";

            Console.WriteLine($"[CHAT LOAD ERROR]: {ex}");
        } finally {
            IsBusy = false;
        }
    }

    private DirectMessageChatListItemViewModel FromCachedChatCreateChatListItemViewModel(CachedChat chat) {
        return new DirectMessageChatListItemViewModel(chat, _chatService, _sessionContext);
    }

    /// <summary>
    /// Handles updates to <see cref="SelectedChat"/> by raising <see cref="ChatSelected"/> 
    /// and resetting the selection state back to <c>null</c> to enable subsequent re-selection.
    /// </summary>
    /// <param name="value">The newly selected chat item ViewModel.</param>
    partial void OnSelectedChatChanged(DirectMessageChatListItemViewModel? value) {
        if (value is not null) {
            ChatSelected?.Invoke(this, value.Chat);

            SelectedChat = null;
        }
    }
}