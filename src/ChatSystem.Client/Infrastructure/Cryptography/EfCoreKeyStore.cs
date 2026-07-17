using System;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Interfaces.Cryptography;
using ChatSystem.Client.Core.Interfaces.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Client.Infrastructure.Cryptography;

/// <summary>
/// Concrete implementation of the IKeyStore using SQLite on EF.
///
/// Persists the encrypted private key material (salt, ciphertext, and KDF algorithm id)
/// safely to a local disk database file.
/// </summary>
internal sealed class EFKeyStore : IKeyStore {
    private readonly ChatSystemLocalDbContext _dbContext;

    private const int KeyIdConst = 1;

    public EFKeyStore(ChatSystemLocalDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task StoreAsync(EncryptedKeyMaterial encryptedKeyMaterial, CancellationToken cancellationToken = default) {
        encryptedKeyMaterial.Id = KeyIdConst;
        encryptedKeyMaterial.StoredAt = DateTimeOffset.UtcNow;

        await _dbContext.KeyStore.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.KeyStore.AddAsync(encryptedKeyMaterial, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EncryptedKeyMaterial?> LoadAsync(CancellationToken cancellationToken = default) {
        var existingKey = await _dbContext.KeyStore.FindAsync([KeyIdConst], cancellationToken);

        return existingKey ?? null;
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default) {
        await _dbContext.KeyStore.ExecuteDeleteAsync(cancellationToken);
    }

    public int KeyId => KeyIdConst;
}