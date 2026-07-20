using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatSystem.Client.Presentation.ViewModels;

internal partial class UserSearchViewModel : ViewModelBase {
    private readonly IUserService _userService;
    private readonly IChatService _chatService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    private CachedUser? _selectedUser;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ObservableCollection<CachedUser> SearchResults { get; } = new();

    public event EventHandler? SearchCancelled;
    public event EventHandler<ChatId>? ChatStarted;

    public UserSearchViewModel(IUserService userService, IChatService chatService) {
        _userService = userService;
        _chatService = chatService;
    }


    private bool CanSearch() {
        return !IsBusy && !string.IsNullOrWhiteSpace(SearchQuery);
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync() {
        IsBusy = true;
        ErrorMessage = null;
        SearchResults.Clear();

        try {
            var users = await _userService.SearchAsync(SearchQuery);

            foreach (var user in users) {
                SearchResults.Add(user);
            }

            if (SearchResults.Count == 0) {
                ErrorMessage = "No users found matching that name.";
            }
        } catch (Exception ex) {
            ErrorMessage = $"Search failed: {ex.Message}";
        } finally {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StartChatAsync(CachedUser targetUser) {
        if (IsBusy) {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try {
            var newChat = await _chatService.CreateChatAsync([targetUser.Id]);

            ChatStarted?.Invoke(this, newChat.Id);
        } catch (Exception ex) {
            ErrorMessage = $"Could not start chat: {ex.Message}";
            IsBusy = false;
        }
    }

    partial void OnSelectedUserChanged(CachedUser? value) {
        if (value is not null) {
            _ = StartChatAsync(value);

            _selectedUser = null;
        }
    }

    [RelayCommand]
    private void Cancel() {
        SearchCancelled?.Invoke(this, EventArgs.Empty);
    }
}