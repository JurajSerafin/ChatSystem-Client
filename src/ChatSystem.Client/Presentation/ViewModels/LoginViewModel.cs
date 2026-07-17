using System;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Interfaces.Presentation;
using ChatSystem.Client.Core.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatSystem.Client.Presentation.ViewModels;

/// <summary>
/// Manages user authentication credentials, sign-in execution, and error handling during login.
/// </summary>
internal partial class LoginViewModel : ViewModelBase {
    private readonly IAuthService _authService;
    private readonly INavigationService _navigation;

    /// <summary>
    /// Gets or sets the username or identifier entered by the user.
    /// </summary>
    [ObservableProperty]
    private string _login = string.Empty;

    /// <summary>
    /// Gets or sets the raw password string entered by the user.
    /// </summary>
    [ObservableProperty]
    private string _password = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether an authentication request is currently in progress.
    /// Used to disable UI controls and display loading indicators.
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Gets or sets the error message displayed to the user if authentication fails.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    /// <summary>
    /// Gets a value indicating whether an error message is currently set.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Occurs when the authentication request completes successfully.
    /// </summary>
    public event EventHandler? LoginSucceeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="authService">The service handling authentication API calls.</param>
    /// <param name="navigation">The service handling view navigation.</param>
    public LoginViewModel(IAuthService authService, INavigationService navigation) {
        _authService = authService;
        _navigation = navigation;
    }

    /// <summary>
    /// Attempts to authenticate the user asynchronously against the backend server.
    /// Handles specific REST API and HTTP network exceptions.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync() {
        IsBusy = true;
        ErrorMessage = null;
        bool loginSuccess = false;

        try {
            await _authService.LoginAsync(Login, Password);
            loginSuccess = true;
        } catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
            ErrorMessage = "Invalid username or password.";
        } catch (Refit.ApiException) {
            ErrorMessage = "Server rejected the request. Try again.";
        } catch (System.Net.Http.HttpRequestException) {
            ErrorMessage = "Cannot reach server. Check your connection.";
        } catch (Exception ex) {
            ErrorMessage = $"Unexpected error: {ex.Message}";
        } finally {
            IsBusy = false;
        }

        if (loginSuccess) {
            try {
                TriggerLoginSuccessForRootSubscribers();
                NavigateToNextScreen();
            } catch (Exception ex) {
                ErrorMessage = $"Navigation has failed to load: {ex.Message}.";
            }
        }
    }

    /// <summary>
    /// Invokes the <see cref="LoginSucceeded"/> event for external observers.
    /// </summary>
    private void TriggerLoginSuccessForRootSubscribers() {
        LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Transitions the application to the initial post-login screen.
    /// </summary>
    private void NavigateToNextScreen() {
        _navigation.NavigateTo<ChatListViewModel>();
    }

    /// <summary>
    /// Determines whether the submit command can execute based on current input validation and busy state.
    /// </summary>
    /// <returns><c>true</c> if form inputs are valid and the service is not busy; otherwise, <c>false</c>.</returns>
    private bool CanSubmit() {
        return !IsBusy
           && !string.IsNullOrWhiteSpace(Login)
           && !string.IsNullOrWhiteSpace(Password);
    }

    partial void OnLoginChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => SubmitCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool value) => SubmitCommand.NotifyCanExecuteChanged();
}