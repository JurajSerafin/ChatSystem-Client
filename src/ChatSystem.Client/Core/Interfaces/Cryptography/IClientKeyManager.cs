namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Interface for a high-level orchestrator/facade for generating, protecting, and unlocking identity keys.
///
/// Bridges the Key Derivation, Encryption, and Key Store services to manage the lifecycle of the user's long-term identity keys.
/// </summary>
internal interface IClientKeyManager {

    /// <summary>
    /// Generates a new key pair, encrypts the private key using the password, and stores it locally.
    /// </summary>
    /// <param name="password">The user's plaintext password.</param>
    /// <returns>The newly generated KeyPair.</returns>
    public KeyPair GenerateAndProtectKeyPair(string password);

    /// <summary>
    /// Derives a key from the password to decrypt and return the user's local private key and holds it in memory.
    /// </summary>
    /// <param name="password">The user's plaintext password.</param>
    public void UnlockPrivateKey(string password);

    /// <summary>
    /// Retrieves the loaded private key.
    /// </summary>
    public string GetPrivateKey();

    /// <summary>
    /// Wipes the locally stored encrypted key material from disk.
    /// </summary>
    public string DeleteProtectedKeys();
}