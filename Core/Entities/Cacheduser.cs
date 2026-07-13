using System;

namespace ChatSystem.Client.Core.Entities;


/// <summary>
/// Represents a user profile cached in the local database
/// </summary>
internal class CachedUser {

    /// <summary>
    /// The user's login username.
    /// </summary>
    public required string Login { get; set; }

    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required string Tag { get; set; }

    /// <summary>
    /// The user's public key, used to wrap symmetric E2EE keys.
    /// </summary>
    public required string PublicKey { get; set; }

    /// <summary>
    /// Timestamp representing the last time this profile has been synced to the local cache.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; set; }
}