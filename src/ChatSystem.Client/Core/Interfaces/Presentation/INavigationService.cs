using ChatSystem.Client.Presentation.ViewModels;

namespace ChatSystem.Client.Core.Interfaces.Presentation {
    /// <summary>
    /// Navigates between top-level ViewModels displayed in the main window.
    /// </summary>
    internal interface INavigationService {
        /// <summary>
        /// Replaces the current view with a new instance of <typeparamref name="TViewModel"/>,
        /// resolved from the DI container.
        /// </summary>
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    }
}
