using System;
using System.ComponentModel.DataAnnotations;

namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Represents the securely packaged material needed to unlock a user's private key.
///
/// This class groups the ciphertext of the private key along with the exact
/// cryptographic parameters needed to reconstruct the decryption key.
/// </summary>
public class EncryptedKeyMaterial {
    /// <summary>
    /// Primary Key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The string identifier containing the KDF algorithm and its parameters.
    /// </summary>
    [MaxLength(32)]
    public required string Algorithm { get; set; }

    /// <summary>
    /// The private key ciphertext, protected by the derived password key.
    /// </summary>
    public required byte[] EncryptedKey { get; set; }

    /// <summary>
    /// he cryptographic salt used during the password key derivation.
    /// </summary>
    public required byte[] Salt { get; set; }

    /// <summary>
    /// Timestamp recording the storage.
    /// </summary>
    public DateTimeOffset StoredAt { get; set; }
}