using System.Threading;
using System.Threading.Tasks;

namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Repository interface for persisting encrypted private key material.
///
/// Typically, abstracts a local caching database, secure enclave, or file system to store the user's encrypted identity.
/// </summary>
internal interface IKeyStore {
    /// <summary>
    /// Saves the encrypted key material to local persistent storage.
    /// </summary>
    /// <param name="encryptedKeyMaterial">The material to be saved.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public Task StoreAsync(EncryptedKeyMaterial encryptedKeyMaterial, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the encrypted key material from local storage.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An EncryptedKeyMaterial instance containing the material if it exists, null otherwise.</returns>
    public Task<EncryptedKeyMaterial?> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the encrypted key material from local storage.
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// </summary>
    public Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the constant primary key identifier used to enforce a single-row constraint 
    /// for the user's encrypted key material in local storage.
    /// </summary>
    public int KeyId { get; }
}