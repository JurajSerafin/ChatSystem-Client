using System.Threading.Tasks;
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
        /// <param name="keyManager">Service managing user cryptographic key lifecycles and storage.</param>
        /// <param name="sidebarPane">The singleton or scoped ViewModel handling the sidebar chat list.</param>
        public ChatShellViewModel(
            INavigationService navigation,
            IClientKeyManager keyManager,
            ChatListViewModel sidebarPane
        ) {
            _navigation = navigation;
            _keyManager = keyManager;
            SidebarPane = sidebarPane;

            _currDetailPane = new EmptyViewModel();
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