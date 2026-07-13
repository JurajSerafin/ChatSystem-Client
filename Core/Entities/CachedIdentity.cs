namespace ChatSystem.Client.Core.Entities;


/// <summary>
/// Represents the actively logged-in user's session and identity.
/// This entity is stored securely and indicates who is currently using the application and
/// holding the session token required for authenticated network requests.
/// </summary>
internal class CachedIdentity {

    /// <summary>
    /// The user's login username.
    /// </summary>
    public required string Login { get; set; }

    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The user's unique display tag or handle.
    /// </summary>
    public required string Tag { get; set; }

    /// <summary>
    /// The active network session token.
    /// </summary>
    public required string SessionToken { get; set; }
}