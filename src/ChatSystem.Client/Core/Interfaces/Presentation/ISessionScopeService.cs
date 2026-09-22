using System;

namespace ChatSystem.Client.Core.Interfaces.Presentation;

/// <summary>
/// Manages a scoped dependency injection lifetime bound to the user's active login session.
///
/// Ensures that scoped services (such as repositories, state managers, and view models) 
/// are instantiated freshly upon login and safely disposed of upon logout.
/// </summary>
internal interface ISessionScopeService {

    /// <summary>
    /// Gets the dependency injection service provider for the currently active user session scope, 
    /// or null if no user session is open.
    /// </summary>
    IServiceProvider? CurrScope { get; }

    /// <summary>
    /// Initializes a new dependency injection scope for an authenticated user session,
    /// safely disposing of any pre-existing session scope first.
    /// </summary>
    /// <returns>The scoped service provider for the newly created session.</returns>
    IServiceProvider BeginSession();

    /// <summary>
    /// Terminates the current user session scope, disposing of all scoped services and clearing the provider reference.
    /// </summary>
    void EndSession();
}