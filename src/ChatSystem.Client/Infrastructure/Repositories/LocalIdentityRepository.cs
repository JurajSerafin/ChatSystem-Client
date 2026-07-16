using ChatSystem.Client.Core.Identity;
using ChatSystem.Client.Core.Interfaces.Database;
using ChatSystem.Client.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatSystem.Client.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of the local identity repository. See <see cref="ILocalIdentityRepository"/>
///
/// Manages the persistence of the active user session and authentication token
/// by delegating to the low-level database engine.
/// </summary>
internal class LocalIdentityRepository : ILocalIdentityRepository {

    private readonly ChatSystemLocalDbContext _dbContext;

    private const int RowCount = 1;

    public LocalIdentityRepository(ChatSystemLocalDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task StoreAsync(CachedIdentity identity, CancellationToken cancellationToken = default) {
        await _dbContext.Identity.ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Identity.AddAsync(identity, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<CachedIdentity?> LoadAsync(CancellationToken cancellationToken = default) {
        return await _dbContext.Identity.FirstOrDefaultAsync(cancellationToken);
    }
    public async Task UpdateSessionTokenAsync(string newToken, CancellationToken cancellationToken = default) {
        var affected = await _dbContext.Identity
            .ExecuteUpdateAsync(
                i => i.SetProperty(x => x.SessionToken, newToken),
                cancellationToken);

        if (affected != RowCount) {
            throw new InvalidOperationException(
                $"Trying to update session token to {affected} identities. Update should always affect {RowCount} rows.");
        }
    }
    public async Task ClearAsync(CancellationToken cancellationToken = default) {
        await _dbContext.Identity.ExecuteDeleteAsync(cancellationToken);
    }
}