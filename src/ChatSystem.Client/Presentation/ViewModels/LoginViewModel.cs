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

    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string? _successMessage;

    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);


    [RelayCommand]
    private void RequestRegistrationNavigation() {
        _navigationService.NavigateTo<RegistrationViewModel>();
    }

    public LoginViewModel(IAuthService authService, INavigationService navigationService) {
        _authService = authService;
        _navigationService = navigationService;
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync() {
        IsBusy = true;

        ErrorMessage = null;

        SuccessMessage = null;

        var loginSuccess = false;

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
            _navigationService.NavigateTo<ChatShellViewModel>();
        }
    }

    private bool CanSubmit() {
        return !IsBusy
           && !string.IsNullOrWhiteSpace(Login)
           && !string.IsNullOrWhiteSpace(Password);
    }

    partial void OnLoginChanged(string value) {
        SubmitCommand.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string value) {
        SubmitCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value) {
        SubmitCommand.NotifyCanExecuteChanged();
    }
}