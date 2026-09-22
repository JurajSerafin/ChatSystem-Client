using ChatSystem.Client.Core.Interfaces.Presentation;
using ChatSystem.Client.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChatSystem.Client.Infrastructure.Presentation;

internal class NavigationService : INavigationService {
    private readonly IServiceProvider _rootProvider;

    private readonly ISessionScopeService _sessionScope;

    public NavigationService(IServiceProvider rootProvider, ISessionScopeService sessionScope) {
        _rootProvider = rootProvider;
        _sessionScope = sessionScope;
    }

    public TViewModel NavigateTo<TViewModel>() where TViewModel : ViewModelBase {
        var provider = _sessionScope.CurrScope ?? _rootProvider;

        var viewModel = provider.GetRequiredService<TViewModel>();

        var mainWindow = _rootProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.CurrentViewModel = viewModel;

        return viewModel;
    }
}