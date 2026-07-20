using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatSystem.Client.Presentation.ViewModels;


internal partial class ChatListViewModel : ViewModelBase {
    private readonly IChatService _chatService;

    public ObservableCollection<CachedChat> Chats { get; set; } = new();
    public ChatListViewModel(IChatService chatService) {
        _chatService = chatService;
    }

    [ObservableProperty]
    private CachedChat? _selectedChat;

    public event EventHandler<CachedChat?> ChatSelected;

    public async Task LoadChatsAsync() {
        Chats.Clear();
        var chats = await _chatService.GetChatsAsync();

        foreach (var chat in chats) {
            Chats.Add(chat);
        }
    }

    partial void OnSelectedChatChanged(CachedChat? value) {
        if (value is not null) {
            ChatSelected?.Invoke(this, value);

            SelectedChat = null;
        }
    }
}