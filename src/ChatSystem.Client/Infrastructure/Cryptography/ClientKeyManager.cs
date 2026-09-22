using ChatSystem.Client.Core.Interfaces.Cryptography;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatSystem.Client.Infrastructure.Cryptography;

/// <summary>
/// Orchestrates the underlying cryptographic, key derivation, and storage services
/// to handle the lifecycle of the user's encrypted identity keys.
/// </summary>
internal class ClientKeyManager : IClientKeyManager {

    private readonly IClientEncryptionService _cryptoService;

    private readonly IKeyDerivationService _keyDerivationService;

    private readonly IKeyStore _keyStore;

    private byte[]? _decryptedPrivateKeyBytes;

    private string? _cachedPublicKey;

    public ClientKeyManager(
        IClientEncryptionService cryptoService,
        IKeyDerivationService keyDerivationService,
        IKeyStore keyStore
    ) {
        _cryptoService = cryptoService;
        _keyDerivationService = keyDerivationService;
        _keyStore = keyStore;
    }

    public async Task<KeyPair> GenerateAndProtectKeyPairAsync(string password, CancellationToken cancellationToken = default) {
        var keyPair = _cryptoService.GenerateKeyPair();

        var salt = _keyDerivationService.GenerateSalt();
        var kdfParams = _keyDerivationService.GetDefaultParams();
        var mek = _keyDerivationService.DeriveKey(password, salt, kdfParams);

        try {
            var encryptedPrivateKey = _cryptoService.EncryptSymmetric(keyPair.PrivateKey, mek);

            await _keyStore.StoreAsync(new EncryptedKeyMaterial {
                Algorithm = kdfParams.ToAlgorithmId(),
                EncryptedKey = encryptedPrivateKey,
                Id = _keyStore.KeyId,
                Salt = salt,
                StoredAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            _decryptedPrivateKeyBytes = Encoding.UTF8.GetBytes(keyPair.PrivateKey);

            return keyPair;
        } finally {
            CryptographicOperations.ZeroMemory(mek);
        }

    }

    public async Task UnlockPrivateKeyAsync(string password, CancellationToken cancellationToken = default) {
        var storedKeyMaterial = await _keyStore.LoadAsync(cancellationToken)
                                ?? throw new InvalidOperationException("No encrypted key material stored.");

        var kdfParams = _keyDerivationService.ParseAlgorithmKdfParams(storedKeyMaterial.Algorithm);

        var mek = _keyDerivationService.DeriveKey(password, storedKeyMaterial.Salt, kdfParams);

        try {
            string decryptedKeyPem = _cryptoService.DecryptSymmetric(storedKeyMaterial.EncryptedKey, mek);
            _decryptedPrivateKeyBytes = Encoding.UTF8.GetBytes(decryptedKeyPem);
        }
        catch (Exception e) {
            throw new InvalidOperationException(
                "Attempt to unlock a stored key was unsuccessful due to incorrect password or corrupted key material", e
            );
        }
        finally {
            CryptographicOperations.ZeroMemory(mek);
        }
    }

    public void LockPrivateKey() {
        if (_decryptedPrivateKeyBytes != null) {
            CryptographicOperations.ZeroMemory(_decryptedPrivateKeyBytes);
            _decryptedPrivateKeyBytes = null;
        }

        _cachedPublicKey = null;
    }

    public string GetPrivateKey() {
        if (_decryptedPrivateKeyBytes == null) {
            throw new InvalidOperationException("Private key that is being returned is unlocked.");
        }

        return Encoding.UTF8.GetString(_decryptedPrivateKeyBytes);
    }

    public string GetPublicKey() {
        _cachedPublicKey ??= _cryptoService.DerivePublicKey(GetPrivateKey());

        return _cachedPublicKey;
    }

    public async Task DeleteProtectedKeysAsync(CancellationToken cancellationToken = default) {
        LockPrivateKey();

        await _keyStore.ClearAsync(cancellationToken);
    }
}