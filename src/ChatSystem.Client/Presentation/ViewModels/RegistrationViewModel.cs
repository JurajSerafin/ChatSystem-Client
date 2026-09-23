using ChatSystem.Client.Core.Interfaces.Presentation;
using ChatSystem.Client.Core.Interfaces.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatSystem.Client.Presentation.ViewModels;

/// <summary>
/// ViewModel managing the user account registration screen.
/// 
/// Handles registration input validation, password confirmation matching
/// and credential server submission
/// </summary>
internal partial class RegistrationViewModel : ViewModelBase {
    private readonly IAuthService _authService;

    private readonly INavigationService _navigationService;
    public RegistrationViewModel(IAuthService authService, INavigationService navigationService) {
        _authService = authService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPassword1))]
    [NotifyPropertyChangedFor(nameof(IsPassword2Enabled))]
    [NotifyPropertyChangedFor(nameof(IsPasswordsMismatch))]
    [NotifyPropertyChangedFor(nameof(IsPasswordsMatch))]
    private string _password1 = string.Empty;

    public bool HasPassword1 => !string.IsNullOrEmpty(Password1);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPassword2))]
    [NotifyPropertyChangedFor(nameof(IsPasswordsMismatch))]
    [NotifyPropertyChangedFor(nameof(IsPasswordsMatch))]
    private string _password2 = string.Empty;


    [ObservableProperty]
    private bool _isBusy;

    public bool HasPassword2 => !string.IsNullOrEmpty(Password2);

    public bool IsPassword2Enabled => HasPassword1 && !IsBusy;

    public bool IsPasswordsMismatch => IsPassword2Enabled && HasPassword2 && Password1 != Password2;

    public bool IsPasswordsMatch => IsPassword2Enabled && HasPassword2 && Password1 == Password2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private string? GetMatchedPassword() {
        if (!string.IsNullOrEmpty(Password1) && !string.IsNullOrEmpty(Password2)) {
            if (Password1 == Password2) {
                return Password1;
            }
        }
        return null;
    }


    /// <summary>
    /// Asynchronously submits the registration request to <see cref="IAuthService"/>, 
    /// handling API exceptions and navigating to <see cref="LoginViewModel"/> upon success.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync() {
        IsBusy = true;

        ErrorMessage = null;

        var registrationSuccess = false;

        try {
            string? matchedPassword = GetMatchedPassword();

            if (matchedPassword is null) {
                ErrorMessage = "Passwords do not match.";
                IsBusy = false;
                return;
            }

            await _authService.RegisterAsync(Login, matchedPassword);

            registrationSuccess = true;
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

        if (registrationSuccess) {
            var loginVm = _navigationService.NavigateTo<LoginViewModel>();

            loginVm.SuccessMessage = $"Account '{Login}' registered successfully! Please log in.";
        }
    }

    /// <summary>
    /// Evaluates whether the submission command can execute.
    /// </summary>
    /// <returns>
    /// true if the view is not busy, <see cref="Login"/> is non-whitespace 
    /// and passwords match; otherwise, false
    /// </returns>
    private bool CanSubmit() {
        return !IsBusy
           && !string.IsNullOrWhiteSpace(Login)
           && !string.IsNullOrWhiteSpace(GetMatchedPassword());
    }


    partial void OnLoginChanged(string value) {
        SubmitCommand.NotifyCanExecuteChanged();
    }

    partial void OnPassword1Changed(string value) {
        if (string.IsNullOrEmpty(Password1)) {
            Password2 = string.Empty;
        }

        SubmitCommand.NotifyCanExecuteChanged();
    }

    partial void OnPassword2Changed(string value) {
        SubmitCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value) {
        SubmitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void NavigateToLogin() {
        _navigationService.NavigateTo<LoginViewModel>();
    }
}