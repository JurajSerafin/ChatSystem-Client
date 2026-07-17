using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatSystem.Client.Presentation.ViewModels;

/// <summary>
/// Serves as the root data context for the main application window.
/// Hosts the active top-level ViewModel to enable single-window navigation.
/// </summary>
internal partial class MainWindowViewModel : ViewModelBase {
    /// <summary>
    /// Gets or sets the ViewModel currently displayed in the main window.
    /// Changing this property causes the root ContentControl to swap the visible view.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="initialViewModel">The initial ViewModel to present upon launch (typically <see cref="LoginViewModel"/>).</param>
    public MainWindowViewModel(LoginViewModel initialViewModel) {
        _currentViewModel = initialViewModel;
    }
}