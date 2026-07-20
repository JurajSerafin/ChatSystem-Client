using ChatSystem.Client.Core.Interfaces.Presentation;
using ChatSystem.Client.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChatSystem.Client.Infrastructure.Presentation;

internal class NavigationService : INavigationService {
    private readonly IServiceProvider _services;
    private readonly MainWindowViewModel _mainWindow;

    public NavigationService(IServiceProvider services, MainWindowViewModel mainWindow) {
        _services = services;
        _mainWindow = mainWindow;
    }

    public void NavigateTo<TViewModel>(Action<TViewModel>? initAction = null) where TViewModel : ViewModelBase {
        CleanupOutgoingViewModelSubscribingMemory();

        ResolveAndAssignNewViewModel<TViewModel>(initAction);
    }

    private void CleanupOutgoingViewModelSubscribingMemory() {
        _mainWindow.CurrentViewModel.Dispose();
    }

    private void ResolveAndAssignNewViewModel<TViewModel>(Action<TViewModel>? initAction = null) where TViewModel : ViewModelBase {
        var viewModel = _services.GetRequiredService<TViewModel>();

        initAction?.Invoke(viewModel);

        _mainWindow.CurrentViewModel = viewModel;
    }

}