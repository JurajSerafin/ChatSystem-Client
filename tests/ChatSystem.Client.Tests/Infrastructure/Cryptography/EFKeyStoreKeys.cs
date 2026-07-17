using System.Threading.Tasks;
using ChatSystem.Client.Core.Interfaces.Cryptography;
using ChatSystem.Client.Infrastructure.Cryptography;
using ChatSystem.Client.Tests.Infrastructure.Repositories;

namespace ChatSystem.Client.Tests.Infrastructure.Cryptography;

public class EfCoreKeyStoreTests : RepositoryTestBase {
    private readonly EFKeyStore _store;

    public EfCoreKeyStoreTests() {
        _store = new EFKeyStore(DbContext);
    }

    private static EncryptedKeyMaterial NewMaterial(
        string algorithm = "argon2id-19456-2-1",
        byte[]? key = null,
        byte[]? salt = null) {
        return new EncryptedKeyMaterial {
            Algorithm = algorithm,
            EncryptedKey = key ?? [1, 2, 3, 4],
            Salt = salt ?? [5, 6, 7, 8]
        };
    }

    [Fact]
    public async Task StoreAsync_EmptyStore_PersistsMaterial() {
        // Arrange
        var material = NewMaterial();

        // Act
        await _store.StoreAsync(material);
        ClearTracker();

        // Assert
        var loaded = await _store.LoadAsync();

        Assert.NotNull(loaded);
        Assert.Equal("argon2id-19456-2-1", loaded.Algorithm);
    }

    [Fact]
    public async Task StoreAsync_ExistingMaterial_ReplacesIt() {
        // Arrange & Act
        await _store.StoreAsync(NewMaterial(algorithm: "old-algo", key: [1, 1, 1]));

        ClearTracker();

        await _store.StoreAsync(NewMaterial(algorithm: "new-algo", key: [2, 2, 2]));

        ClearTracker();

        // Assert
        var loaded = await _store.LoadAsync();

        Assert.Equal("new-algo", loaded!.Algorithm);
        Assert.Equal(new byte[] { 2, 2, 2 }, loaded.EncryptedKey);
    }


    [Fact]
    public async Task ClearAsync_RemovesStoredMaterial() {
        // Arrange
        await _store.StoreAsync(NewMaterial());

        // Act
        await _store.ClearAsync();
        ClearTracker();

        // Assert
        var loaded = await _store.LoadAsync();
        Assert.Null(loaded);
    }

    [Fact]
    public async Task ClearAsync_EmptyStore_DoesNotThrow() {
        // Arrange & Act & Assert
        await _store.ClearAsync();
    }
}