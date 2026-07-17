using System;
using System.Security.Cryptography;
using System.Text;
using ChatSystem.Client.Core.Interfaces.Cryptography;

namespace ChatSystem.Client.Infrastructure.Cryptography;

/// <summary>
/// Concrete implementation of the encryption service using .NET's System.Security.Cryptography.
///
/// Handles high-level cryptographic operations including RSA key generation,
/// AES symmetric encryption, and secure key wrapping/unwrapping.
/// </summary>
internal sealed class AesRsaEncryptionService : IClientEncryptionService {

    private const int AesKeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public KeyPair GenerateKeyPair() {
        var rsa = RSA.Create();

        return new KeyPair {
            PrivateKey = rsa.ExportRSAPrivateKeyPem(),
            PublicKey = rsa.ExportRSAPublicKeyPem()
        };
    }
    public byte[] GenerateSymmetricKey() {
        return RandomNumberGenerator.GetBytes(AesKeySize);
    }
    public byte[] EncryptSymmetric(string plaintext, byte[] key) {
        ValidateAesKeySize(key);

        var plainTextBytes = Encoding.UTF8.GetBytes(plaintext);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);

        var ciphertext = new byte[plainTextBytes.Length];

        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plainTextBytes , ciphertext, tag);

        return PackAesAndReturn(nonce, ciphertext, tag);
    }

    public string DecryptSymmetric(byte[] ciphertext, byte[] key) {
        ValidateAesKeySize(key);

        if (ciphertext.Length < NonceSize + TagSize) {
            throw new CryptographicException("Unable to decrypt: length of a ciphertext is too short to contain nonce and tag.");
        }

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var pureCiphertextLen = ciphertext.Length - NonceSize - TagSize;
        var pureCiphertextBytes = new byte[pureCiphertextLen];
        var plaintext = new byte[pureCiphertextLen];

        Buffer.BlockCopy(ciphertext, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(ciphertext, NonceSize + TagSize, pureCiphertextBytes, 0, pureCiphertextLen);

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, pureCiphertextBytes, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
    public byte[] WrapKey(byte[] symmetricKey, string publicKeyPem) {
        using var rsa = RSA.Create();

        rsa.ImportFromPem(publicKeyPem);

        return rsa.Encrypt(symmetricKey, RSAEncryptionPadding.OaepSHA256);
    }
    public byte[] UnwrapKey(byte[] wrappedKey, string privateKeyPem) {
        using var rsa = RSA.Create();

        rsa.ImportFromPem(privateKeyPem);

        return rsa.Decrypt(wrappedKey, RSAEncryptionPadding.OaepSHA256);
    }
    public byte[] Sign(byte[] data, string privateKey) {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKey);

        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
    public bool Verify(byte[] data, byte[] signature, string publicKeyPem) {
        using var rsa = RSA.Create();

        rsa.ImportFromPem(publicKeyPem);

        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
    private byte[] PackAesAndReturn(byte[] nonce, byte[] ciphertext, byte[] tag) {
        var package = new byte[NonceSize + TagSize + ciphertext.Length];

        Buffer.BlockCopy(nonce, 0, package, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, package, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, package, NonceSize + TagSize, ciphertext.Length);

        return package;
    }

    private static void ValidateAesKeySize(byte[] key) {
        if (key.Length != AesKeySize) {
            throw new ArgumentException(
                $"Proposed AES key does not match the required length. Need {AesKeySize} bytes, got {key.Length} instead.",
                nameof(key));
        }
    }
}