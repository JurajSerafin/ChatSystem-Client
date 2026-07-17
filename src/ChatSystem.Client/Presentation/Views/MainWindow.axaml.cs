using Avalonia.Controls;

namespace ChatSystem.Client.Presentation.Views;

/// <summary>
/// The primary desktop window of the application.
/// Hosts the root <see cref="ContentControl"/> responsible for switching between top-level ViewModels.
/// </summary>
public partial class MainWindow : Window {
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class and loads its XAML components.
    /// </summary>
    public MainWindow() {
        InitializeComponent();
    }
}