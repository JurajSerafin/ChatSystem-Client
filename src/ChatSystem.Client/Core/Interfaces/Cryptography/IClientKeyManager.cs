using System.Threading;
using System.Threading.Tasks;

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
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The newly generated KeyPair.</returns>
    public Task<KeyPair> GenerateAndProtectKeyPairAsync(string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Derives a key from the password to decrypt and return the user's local private key and holds it in memory.
    /// </summary>
    /// <param name="password">The user's plaintext password.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public Task UnlockPrivateKeyAsync(string password, CancellationToken cancellationToken = default);

    public void LockPrivateKey();

    /// <summary>
    /// Retrieves the loaded private key.
    /// </summary>
    public string GetPrivateKey();

    /// <summary>
    /// Wipes the locally stored encrypted key material from disk.
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// </summary>
    public Task DeleteProtectedKeysAsync(CancellationToken cancellationToken = default);
}