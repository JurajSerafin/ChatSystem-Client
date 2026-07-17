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

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase {
        CleanupOutgoingViewModelSubscribingMemory();

        ResolveAndAssignNewViewModel<TViewModel>();
    }

    private void CleanupOutgoingViewModelSubscribingMemory() {
        _mainWindow.CurrentViewModel.Dispose();
    }

    private void ResolveAndAssignNewViewModel<TViewModel>() where TViewModel : ViewModelBase {
        var viewModel = _services.GetRequiredService<TViewModel>();
        _mainWindow.CurrentViewModel = viewModel;
    }

}