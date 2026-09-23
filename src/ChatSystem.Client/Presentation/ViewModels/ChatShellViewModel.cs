using System;
using System.Linq;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Interfaces.Presentation;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ChatSystem.Client.Presentation.ViewModels;


/// <summary>
/// Orchestrates the primary two-pane messaging UI after successful authentication.
/// 
/// Maintains a sidebar navigation pane while dynamically swapping out the detail pane content.
/// </summary>
internal partial class ChatShellViewModel : ViewModelBase {
    private readonly INavigationService _navigationService;
    
    private readonly IAuthService _authService;
        
    private readonly ISessionScopeService _sessionScopeService;

    /// <summary>
    /// Gets the ViewModel representing the left-hand sidebar list of chats.
    /// This remains constant for the lifespan of the shell.
    /// </summary>
    public ChatListViewModel SidebarPane { get; }

    /// <summary>
    /// Gets or sets the ViewModel currently active in the right-hand detail pane.
    /// Changing this property notifies the UI to dynamically swap the visible conversation
    /// or search view.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _currDetailPane;

    public ChatShellViewModel(
        INavigationService navigation,
        ChatListViewModel sidebarPane,
        IAuthService authService,
        ISessionScopeService sessionScopeService
    ) {
        _navigationService = navigation;
        SidebarPane = sidebarPane;
        _sessionScopeService = sessionScopeService;
        _authService = authService;
        _currDetailPane = new EmptyViewModel();

        SidebarPane.ChatSelected += OnSidebarChatSelected!;
        _ = SidebarPane.LoadChatsAsync();
    }

    /// <summary>
    /// Gets the active session-scoped service provider for resolving session-bound ViewModels.
    /// </summary>
    private IServiceProvider CurrProvider => _sessionScopeService.CurrScope
        ?? throw new InvalidOperationException("No active session scope.");


    [RelayCommand]
    private void OpenSearch() {
        DisposeMiddlePane();

        var searchVm = ActivatorUtilities.CreateInstance<UserSearchViewModel>(CurrProvider);
            
        WireUpCancelSearch(searchVm);
        WireUpStartChat(searchVm);

        CurrDetailPane = searchVm;
    }

    [RelayCommand]
    private async Task LogoutAsync() {
        await _authService.LogoutAsync();

        _navigationService.NavigateTo<LoginViewModel>();
    }

    private void WireUpCancelSearch(UserSearchViewModel searchVm) {
        searchVm.SearchCancelled += (_, _) => {
            searchVm.Dispose();
            CurrDetailPane = new EmptyViewModel();
        };
    }

    private void WireUpStartChat(UserSearchViewModel searchVm) {
        searchVm.ChatStarted += async (_, newChatId) => {
            searchVm.Dispose();

            await SidebarPane.LoadChatsAsync();

            var selectedChatItemVm = FetchSelectedChatListItemVm(newChatId);

            if (selectedChatItemVm is not null) {
                var selectedChatVm = CreateChatVmInstance(selectedChatItemVm);

                WireUpCloseChatAction(selectedChatVm);

                CurrDetailPane = selectedChatVm;
            } else {
                CurrDetailPane = new EmptyViewModel();
            }
        };
    }

    private DirectMessageChatListItemViewModel? FetchSelectedChatListItemVm(ChatId newChatId) {
        return SidebarPane.Chats.FirstOrDefault(
            chatListItemViewModel => chatListItemViewModel.Chat.Id == newChatId
        );
    }

    private ChatViewModel CreateChatVmInstance(DirectMessageChatListItemViewModel chatListItemVm) {
        return ActivatorUtilities.CreateInstance<ChatViewModel>(CurrProvider, chatListItemVm.Chat);
    }

    private ChatViewModel CreateChatVmInstance(CachedChat chat) {
        return ActivatorUtilities.CreateInstance<ChatViewModel>(CurrProvider, chat);
    }

    private void OnSidebarChatSelected(object? sender, CachedChat selectedChat) {
        DisposeMiddlePane();

        var selectedChatVm = CreateChatVmInstance(selectedChat);

        WireUpCloseChatAction(selectedChatVm);

        CurrDetailPane = selectedChatVm;
    }

    private void WireUpCloseChatAction(ChatViewModel chatVm) {
        chatVm.CloseRequested += (_, _) => {
            chatVm.Dispose();
            CurrDetailPane = new EmptyViewModel();
        };

    }

    private void DisposeMiddlePane() {
        CurrDetailPane?.Dispose();
    }
}