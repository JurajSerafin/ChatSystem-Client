using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatSystem.Client.Presentation.ViewModels;

/// <summary>
/// The root ViewModel driving the main application window.
/// 
/// Holds and swaps the ViewModels/screens.
/// </summary>
internal partial class MainWindowViewModel : ViewModelBase {
    [ObservableProperty]
    private ViewModelBase _currentViewModel = null!;
}