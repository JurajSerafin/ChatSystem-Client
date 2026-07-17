using Avalonia.Controls;

namespace ChatSystem.Client.Presentation.Views;

/// <summary>
/// A user control that defines the overarching master-detail layout grid (sidebar, separator line, and dynamic content area) for
/// the chat application.
/// </summary>
public partial class ChatShellView : UserControl {
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatShellView"/> class and loads its XAML components.
    /// </summary>
    public ChatShellView() {
        InitializeComponent();
    }
}