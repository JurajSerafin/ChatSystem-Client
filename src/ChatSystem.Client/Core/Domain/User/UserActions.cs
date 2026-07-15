namespace ChatSystem.Client.Core.Domain.User;


/// <summary>
/// Enumerates all possible actions a user can perform in the system.
///
/// This enum defines the full set of operations that can be checked
/// against user roles for authorization purposes.
///
/// Each action represents a specific capability that may be granted or denied
/// depending on the user's role.
/// </summary>
internal enum UserActions {

    /// <summary>
    /// Send a message to another user or chat.
    /// </summary>
    SendMessage,

    /// <summary>
    ///  Delete the user's own account.
    /// </summary>
    DeleteAccount,

    /// <summary>
    /// Modify the user's login/username.
    /// </summary>
    ModifyLogin,

    /// <summary>
    /// Modify the user's password.
    /// </summary>
    ModifyPassword,

    /// <summary>
    /// Modify the user's tag/identifier.
    /// </summary>
    ModifyTag,

    /// <summary>
    /// Delete a message owned by the user.
    /// </summary>
    DeleteOwnMessage,

    /// <summary>
    /// Delete any message regardless of ownership.
    /// </summary>
    DeleteAnyMessage,

    /// <summary>
    /// Ban a user from the system.
    /// </summary>
    BanUser,

    /// <summary>
    /// Manage user roles and permissions.
    /// </summary>
    ManegeRoles
}