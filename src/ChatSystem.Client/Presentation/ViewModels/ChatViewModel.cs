using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Interfaces.Services;
using ChatSystem.Client.Core.Interfaces.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatSystem.Client.Presentation.ViewModels {
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

        public Guid CurrentUserId => _sessionContext.CurrentUserId?.Value ?? Guid.Empty;

        public ObservableCollection<CachedMessage> Messages { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private string _messageText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private bool _isBusy;

        [ObservableProperty]
        private string? _errorMessage;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // ChatShell listens to this for the middle screen swaping
        public event EventHandler? CloseRequested;

        public async Task LoadHistoryAsync() {
            IsBusy = true;
            ErrorMessage = null;
            Messages.Clear();

            try {
                var history = await _messageService.GetHistoryAsync(CurrentChat.Id);

                foreach (var message in history) {
                    Messages.Add(message);
                }
            } catch (Exception ex) {
                ErrorMessage = $"Failed to load messages: {ex.Message}";
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

                Messages.Add(sentMessage);
            } catch (Exception ex) {
                ErrorMessage = $"Failed to send message: {ex.Message}";

                MessageText = textToSend;
            }
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
}
