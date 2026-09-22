namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Interface for a low-level cryptographic engine for symmetric/asymmetric encryption and signatures.
///
/// Abstracts the underlying cryptographic library, providing primitive operations for the End-to-End Encryption (E2EE) pipeline.
/// </summary>
internal interface IClientEncryptionService {

    /// <summary>
    /// Generates a fresh asymmetric public/private key pair.
    /// </summary>
    /// <returns>A KeyPair instance containing the generated keys.</returns>
    public KeyPair GenerateKeyPair();

    /// <summary>
    /// Generates a cryptographically secure symmetric key.
    /// </summary>
    /// <returns>A raw byte string representing the symmetric key (e.g., AES-256).</returns>
    public byte[] GenerateSymmetricKey();

    /// <summary>
    /// Encrypts plaintext using the provided symmetric key.
    /// </summary>
    /// <param name="plaintext">The unencrypted data.</param>
    /// <param name="key">The symmetric key to encrypt with.</param>
    /// <returns>The resulting ciphertext.</returns>
    public byte[] EncryptSymmetric(string plaintext, byte[] key);

    /// <summary>
    /// Decrypts ciphertext back to plaintext using the provided symmetric key.
    /// </summary>
    /// <param name="ciphertext">The encrypted data.</param>
    /// <param name="key">The symmetric key to decrypt with.</param>
    /// <returns>The resulting plaintext.</returns>
    public string DecryptSymmetric(byte[] ciphertext, byte[] key);

    /// <summary>
    /// Wraps (encrypts) a symmetric key using a recipient's public asymmetric key.
    /// </summary>
    /// <param name="symmetricKey">The payload key to be wrapped.</param>
    /// <param name="publicKeyPem"></param>
    /// <returns>The asymmetrically encrypted symmetric key.</returns>
    public byte[] WrapKey(byte[] symmetricKey, string publicKeyPem);

    /// <summary>
    /// Unwraps (decrypts) a symmetric key using the user's private asymmetric key.
    /// </summary>
    /// <param name="wrappedKey">The asymmetrically encrypted symmetric key.</param>
    /// <param name="privateKeyPem"></param>
    /// <returns>The raw, unencrypted symmetric key.</returns>
    public byte[] UnwrapKey(byte[] wrappedKey, string privateKeyPem);

    /// <summary>
    /// Generates a cryptographic signature for the given data using a private key.
    /// </summary>
    /// <param name="data">The payload to sign.</param>
    /// <param name="privateKey">The signer's private key.</param>
    /// <returns>The cryptographic signature.</returns>
    public byte[] Sign(byte[] data, string privateKey);

    /// <summary>
    /// Verifies a cryptographic signature against the original data using a public key.
    /// </summary>
    /// <param name="data">The original payload.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="publicKeyPem"></param>
    /// <returns>True if the signature is valid, false otherwise.</returns>
    public bool Verify(byte[] data, byte[] signature, string publicKeyPem);

    /// <summary>
    /// Derives the public key from a given private key in PEM format.
    /// </summary>
    public string DerivePublicKey(string privateKeyPem);
}