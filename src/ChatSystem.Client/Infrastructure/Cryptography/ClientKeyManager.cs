using System;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Interfaces.Cryptography;

namespace ChatSystem.Client.Infrastructure.Cryptography;

/// <summary>
/// Orchestrates the underlying cryptographic, key derivation, and storage services
/// to handle the lifecycle of the user's encrypted identity keys.
/// </summary>
internal class ClientKeyManager : IClientKeyManager {

    private readonly IClientEncryptionService _cryptoService;

    private readonly IKeyDerivationService _keyDerivationService;

    private readonly IKeyStore _keyStore;

    private string? _decryptedPrivateKey;

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

        var encryptedPrivateKey = _cryptoService.EncryptSymmetric(keyPair.PrivateKey, mek);

        await _keyStore.StoreAsync(new EncryptedKeyMaterial {
            Algorithm = kdfParams.ToAlgorithmId(),
            EncryptedKey = encryptedPrivateKey,
            Id = _keyStore.KeyId,
            Salt = salt,
            StoredAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        _decryptedPrivateKey = keyPair.PrivateKey;

        return keyPair;
    }

    public async Task UnlockPrivateKeyAsync(string password, CancellationToken cancellationToken = default) {
        var storedKeyMaterial = await _keyStore.LoadAsync(cancellationToken)
                                ?? throw new InvalidOperationException("No encrypted key material stored.");

        var kdfParams = _keyDerivationService.ParseAlgorithmKdfParams(storedKeyMaterial.Algorithm);

        var mek = _keyDerivationService.DeriveKey(password, storedKeyMaterial.Salt, kdfParams);

        try {
            _decryptedPrivateKey = _cryptoService.DecryptSymmetric(storedKeyMaterial.EncryptedKey, mek);
        }
        catch (Exception e) {
            throw new InvalidOperationException(
                "Attempt to unlock a stored key was unsuccessful due to incorrect password or corrupted key material", e
            );
        }
    }

    public void LockPrivateKey() {
        _decryptedPrivateKey = null;
    }

    public string GetPrivateKey() {
        return _decryptedPrivateKey
               ?? throw new InvalidOperationException("Private key that is being returned is unlocked.");
    }

    public async Task DeleteProtectedKeysAsync(CancellationToken cancellationToken = default) {
        LockPrivateKey();

        await _keyStore.ClearAsync(cancellationToken);
    }
}