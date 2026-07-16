using System;
using System.ComponentModel.DataAnnotations;

namespace ChatSystem.Client.Core.Domain.User;


/// <summary>
/// Represents a user profile cached in the local database
/// </summary>
internal class CachedUser {

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
    /// The user's unique display tag.
    /// </summary>
    [MaxLength(64)]
    public required string Tag { get; set; }

    /// <summary>
    /// The user's public key, used to wrap symmetric E2EE keys.
    /// </summary>
    [MaxLength(2048)]
    public required string PublicKey { get; set; }

    /// <summary>
    /// The string token representing a user's role, defining a set of authorized actions. See <see cref="IUserRole"/>
    /// </summary>
    [MaxLength(32)]
    public required string RoleString { get; set; }

    /// <summary>
    /// Timestamp representing the last time this profile has been synced to the local cache.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; set; }
}