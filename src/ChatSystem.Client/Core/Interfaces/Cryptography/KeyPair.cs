namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Represents a cryptographic key pair.
///
/// This struct stores a public/private key pair used for authentication and encryption-related operations.
/// Instances are immutable after construction except for move operations.
/// </summary>
/// <param name="PublicKey">Public key</param>
/// <param name="PrivateKey">Private key</param>
internal readonly record struct KeyPair(string PublicKey, string PrivateKey) {

    /// <summary>
    /// Compares two key pairs for equality, only the public key is used for comparison.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(KeyPair other) {
        return PublicKey == other.PublicKey;
    }

    /// <summary>
    /// Generates a string-based hash code from the public key.
    /// </summary>
    /// <returns>KeyPair hashcode derived from the public key.</returns>
    public override int GetHashCode() {
        return string.GetHashCode(PublicKey);
    }
}