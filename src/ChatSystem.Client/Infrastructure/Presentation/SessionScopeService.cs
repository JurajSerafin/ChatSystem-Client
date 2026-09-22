using System;
using ChatSystem.Client.Core.Interfaces.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace ChatSystem.Client.Infrastructure.Presentation;

/// <summary>
/// Manages a scoped dependency injection lifetime bound to the user's active login session.
///
/// Ensures that scoped services (such as repositories, state managers, and view models) 
/// are instantiated freshly upon login and safely disposed of upon logout.
/// </summary>
internal sealed class SessionScopeService : ISessionScopeService {

    private readonly IServiceProvider _rootProvider;

    private IServiceScope? _sessionScope;

    public SessionScopeService(IServiceProvider serviceProvider) {
        _rootProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IServiceProvider? CurrScope => _sessionScope?.ServiceProvider;

    /// <inheritdoc />
    public IServiceProvider BeginSession() {
        _sessionScope?.Dispose();
        _sessionScope = _rootProvider.CreateScope();

        return _sessionScope.ServiceProvider;
    }

    /// <inheritdoc />
    public void EndSession() {
        _sessionScope?.Dispose();
        _sessionScope = null;
    }
}