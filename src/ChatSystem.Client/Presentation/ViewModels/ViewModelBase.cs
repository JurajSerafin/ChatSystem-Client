using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatSystem.Client.Presentation.ViewModels;

/// <summary>
/// Base class for all ViewModels. Inherits from <see cref="ObservableObject"/>
/// which provides automatic INotifyPropertyChanged support required by data binding.
/// </summary>
internal abstract class ViewModelBase : ObservableObject, IDisposable
{
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting resources.
    /// Override in derived classes to unsubscribe from network events or clear cryptographic buffers.
    /// </summary>
    public virtual void Dispose() {}
}
