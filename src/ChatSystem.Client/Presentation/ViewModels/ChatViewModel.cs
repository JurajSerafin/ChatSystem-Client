using Avalonia.Threading;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ChatSystem.Client.Presentation.ViewModels;

/// <summary>
/// ViewModel representing an active chat conversation panel.
/// 
/// Handles loading message history, sending encrypted messages, 
/// managing UI loading states, and broadcasting close requests.
/// </summary>
internal partial class ChatViewModel : ViewModelBase {
    private readonly IMessageService _messageService;

    private readonly ISessionContext _sessionContext;

    public CachedChat CurrentChat { get; }

    public ChatViewModel(IMessageService messageService, ISessionContext sessionContext, CachedChat currentChat) {
        _messageService = messageService;
        _sessionContext = sessionContext;
        CurrentChat = currentChat;

        _ = LoadHistoryAsync();
    }

    public UserId CurrentUserId => _sessionContext.CurrentUserId ?? default;

    public ObservableCollection<CachedMessage> Messages { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private string _messageText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public event EventHandler? CloseRequested;

    public async Task LoadHistoryAsync() {
        IsBusy = true;
        ErrorMessage = null;

        EnqueActionToUIThread(() => Messages.Clear());

        try {
            var history = await _messageService.GetHistoryAsync(CurrentChat.Id);

            var chronoSortedHistory = history.OrderBy(message => message.CreatedAt);

            EnqueActionToUIThread(() => {
                foreach (var message in chronoSortedHistory) {
                    Messages.Add(message);
                }
            });

        } catch (Exception ex) {
            ErrorMessage = $"Failed to load messages: {ex.Message}";

            Console.WriteLine(ErrorMessage);
        } finally {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync() {
        var textToSend = MessageText.Trim();

        MessageText = string.Empty;

        ErrorMessage = null;

        try {
            var sentMessage = await _messageService.SendMessageAsync(CurrentChat.Id, textToSend);

            EnqueActionToUIThread(() => Messages.Add(sentMessage));

        } catch (Exception ex) {
            ErrorMessage = $"Failed to send message: {ex.Message}";

            MessageText = textToSend;
        }
    }

    private void EnqueActionToUIThread(Action action) {
        Dispatcher.UIThread.Post(action);
    }

    [RelayCommand]
    private void Close() {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanSendMessage() {
        return !IsBusy && !string.IsNullOrWhiteSpace(MessageText);
    }

    partial void OnMessageTextChanged(string value) {
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value) {
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    public override void Dispose() {
        base.Dispose();

        Messages.Clear();
    }
}