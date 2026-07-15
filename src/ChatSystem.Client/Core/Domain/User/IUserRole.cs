namespace ChatSystem.Client.Core.Domain.User;

/// <summary>
/// Interface  defining a contract for a User Role.
///
/// Each role represents a set of actions, each specifying a capability
/// that may be granted or denied depending on the user's role.
/// </summary>
internal interface IUserRole {

    /// <summary>
    /// Provides a static string identifier for the role type (e.g., "ADMIN").
    /// </summary>
    /// <returns></returns>
    static abstract string GetTypeString();

    /// <summary>
    /// Evaluate whether this role has permission to execute a specific action.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    static abstract bool CanPerform(UserActions action);
}