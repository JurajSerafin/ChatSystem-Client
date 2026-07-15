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
    public void Store(EncryptedKeyMaterial encryptedKeyMaterial);

    /// <summary>
    /// Retrieves the encrypted key material from local storage.
    /// </summary>
    /// <returns>An EncryptedKeyMaterial instance containing the material if it exists, null otherwise.</returns>
    public EncryptedKeyMaterial? Load();

    /// <summary>
    /// Deletes the encrypted key material from local storage.
    /// </summary>
    public void Clear();
}