using System;
using System.Linq;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Interfaces.Cryptography;
using ChatSystem.Client.Core.Interfaces.Presentation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatSystem.Client.Presentation.ViewModels {
    /// <summary>
    /// Orchestrates the primary two-pane master-detail messaging interface after successful authentication.
    /// Maintains a persistent sidebar navigation pane while dynamically swapping out the detail pane content.
    /// </summary>
    internal partial class ChatShellViewModel : ViewModelBase {
        private readonly INavigationService _navigation;
        private readonly IClientKeyManager _keyManager;

        private readonly Func<UserSearchViewModel> _userSearchFactory;
        private readonly Func<CachedChat, ChatViewModel> _chatViewModelFactory;


        /// <summary>
        /// Gets the persistent ViewModel driving the left-hand sidebar list of chats.
        /// This reference remains constant for the lifespan of the shell.
        /// </summary>
        public ChatListViewModel SidebarPane { get; }
        /// <summary>
        /// Gets or sets the ViewModel currently active in the right-hand detail pane.
        /// Changing this property notifies the UI to dynamically swap the visible conversation or search view.
        /// </summary>
        [ObservableProperty]
        private ViewModelBase _currDetailPane;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatShellViewModel"/> class.
        /// </summary>
        /// <param name="navigation">Service used to navigate between top-level application screens.</param>
        /// <param name="sidebarPane">The singleton or scoped ViewModel handling the sidebar chat list.</param>
        /// <param name="keyManager"></param>
        /// <param name="userSearchFactory"></param>
        /// <param name="chatViewModelFactory"></param>
        public ChatShellViewModel(
            INavigationService navigation,
            ChatListViewModel sidebarPane,
            IClientKeyManager keyManager,
            Func<UserSearchViewModel> userSearchFactory,
            Func<CachedChat, ChatViewModel> chatViewModelFactory
        ) {
            _navigation = navigation;
            SidebarPane = sidebarPane;
            _keyManager = keyManager;
            _userSearchFactory = userSearchFactory;
            _chatViewModelFactory = chatViewModelFactory;
            _currDetailPane = new EmptyViewModel();

            SidebarPane.ChatSelected += OnSidebarChatSelected!;
        }

        [RelayCommand]
        private void OpenSearch() {
            var searchVm = _userSearchFactory();

            WireUpCancelSearch(searchVm);

            WireUpStartChat(searchVm);

            CurrDetailPane = searchVm;

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

                var newChat = SidebarPane.Chats.FirstOrDefault(c => c.Id == newChatId);

                if (newChat != null) {
                    var newChatVm = _chatViewModelFactory(newChat);

                    WireUpCloseChatAction(newChatVm);

                    CurrDetailPane = newChatVm;
                } else {
                    CurrDetailPane = new EmptyViewModel();

                }
            };
        }

        private void OnSidebarChatSelected(object? sender, CachedChat selectedChat) {
            DisposeMiddlePane();

            var chatVm = _chatViewModelFactory(selectedChat);

            WireUpCloseChatAction(chatVm);

            CurrDetailPane = chatVm;
        }

        private void WireUpCloseChatAction(ChatViewModel chatVm) {
            chatVm.CloseRequested += (_, _) => {
                chatVm.Dispose();
                CurrDetailPane = new EmptyViewModel();
            };
        }

        private void DisposeMiddlePane() {
            _currDetailPane?.Dispose();
        }

        /// <summary>
        /// Terminates the current user session by wiping protected private keys from storage and memory,
        /// then navigating back to the login screen.
        /// </summary>
        [RelayCommand]
        private async Task LogoutAsync() {
            await _keyManager.DeleteProtectedKeysAsync();
            NavigateAfterLogout();
        }

        /// <summary>
        /// Executes top-level navigation to return the user to the authentication screen.
        /// </summary>
        private void NavigateAfterLogout() {
            _navigation.NavigateTo<LoginViewModel>();
        }
    }
}