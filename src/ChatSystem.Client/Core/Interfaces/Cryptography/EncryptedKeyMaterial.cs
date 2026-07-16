using System.Collections.Generic;

namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Represents the securely packaged material needed to unlock a user's private key.
///
/// This struct groups the ciphertext of the private key along with the exact
/// cryptographic parameters needed to reconstruct the decryption key.
/// </summary>
/// <param name="Salt">The cryptographic salt used during the password key derivation.</param>
/// <param name="EncryptedKey">The private key ciphertext, protected by the derived password key.</param>
/// <param name="Algorithm">The string identifier containing the KDF algorithm and its parameters.</param>
internal readonly record struct EncryptedKeyMaterial(
    List<byte> Salt,
    List<byte> EncryptedKey,
    string Algorithm
);