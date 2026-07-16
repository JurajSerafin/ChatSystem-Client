using System.ComponentModel.DataAnnotations;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Identity;


/// <summary>
/// Represents the actively logged-in user's session and identity.
/// This entity is stored securely and indicates who is currently using the application and
/// holding the session token required for authenticated network requests.
/// </summary>
internal class CachedIdentity {

    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required UserId Id { get; set; }

    /// <summary>
    /// The user's login username.
    /// </summary>
    [MaxLength(64)]
    public required string Login { get; set; }

    /// <summary>
    /// The user's unique display tag or handle.
    /// </summary>
    [MaxLength(64)]
    public required string Tag { get; set; }

    /// <summary>
    /// The active network session token.
    /// </summary>
    [MaxLength(512)]
    public required string SessionToken { get; set; }
}