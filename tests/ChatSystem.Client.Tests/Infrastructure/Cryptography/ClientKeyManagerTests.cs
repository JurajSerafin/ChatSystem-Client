using System;
using System.Threading.Tasks;
using ChatSystem.Client.Infrastructure.Cryptography;
using ChatSystem.Client.Tests.Infrastructure.Repositories;

namespace ChatSystem.Client.Tests.Infrastructure.Cryptography;

public class ClientKeyManagerTests : RepositoryTestBase {
    private readonly ClientKeyManager _manager;

    public ClientKeyManagerTests() {
        var encryptionService = new AesRsaEncryptionService();
        var kdfService = new Argon2KeyDerivationService();
        var keyStore = new EFKeyStore(DbContext);

        _manager = new ClientKeyManager(encryptionService, kdfService, keyStore);
    }

    [Fact]
    public async Task UnlockPrivateKey_CorrectPassword_RestoresPrivateKey() {
        // Arrange
        var keyPair = await _manager.GenerateAndProtectKeyPairAsync("password123");

        _manager.LockPrivateKey();

        ClearTracker();

        // Act
        await _manager.UnlockPrivateKeyAsync("password123");

        // Assert
        Assert.Equal(keyPair.PrivateKey, _manager.GetPrivateKey());
    }

    [Fact]
    public async Task UnlockPrivateKey_WrongPassword_ThrowsInvalidOperationException() {
        // Arrange
        await _manager.GenerateAndProtectKeyPairAsync("password123");

        _manager.LockPrivateKey();

        ClearTracker();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _manager.UnlockPrivateKeyAsync("wrong-password"));
    }

    [Fact]
    public async Task GetPrivateKey_WhenLocked_ThrowsInvalidOperationException() {
        // Arrange
        await _manager.GenerateAndProtectKeyPairAsync("password123");

        _manager.LockPrivateKey();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _manager.GetPrivateKey());
    }

    [Fact]
    public async Task DeleteProtectedKeys_ClearsBothMemoryAndStorage() {
        // Arrange
        const string passw = "password123";
        await _manager.GenerateAndProtectKeyPairAsync(passw);

        // Act
        await _manager.DeleteProtectedKeysAsync();
        ClearTracker();

        // Assert
        Assert.Throws<InvalidOperationException>(() => _manager.GetPrivateKey());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _manager.UnlockPrivateKeyAsync(passw));
    }

    [Fact]
    public async Task LockAndUnlock_MultipleTimes_KeyRemainsConsistent() {
        // Arrange
        const string passw = "password123";

        var keyPair = await _manager.GenerateAndProtectKeyPairAsync(passw);

        var originalKey = keyPair.PrivateKey;

        // Act & Assert
        _manager.LockPrivateKey();

        ClearTracker();

        await _manager.UnlockPrivateKeyAsync(passw);

        Assert.Equal(originalKey, _manager.GetPrivateKey());

        // Act & Assert
        _manager.LockPrivateKey();

        ClearTracker();

        await _manager.UnlockPrivateKeyAsync(passw);

        Assert.Equal(originalKey, _manager.GetPrivateKey());
    }
}