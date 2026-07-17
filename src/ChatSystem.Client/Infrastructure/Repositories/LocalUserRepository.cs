using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Database;
using ChatSystem.Client.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Client.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of the local user repository. See <see cref="ILocalUserRepository"/>
///
/// Handles caching and retrieving public user profiles, ensuring cryptographic
/// public keys are accessible on a short notice, without making redundant network calls.
/// </summary>
internal class LocalUserRepository : ILocalUserRepository {

    private readonly ChatSystemLocalDbContext _dbContext;

    public LocalUserRepository(ChatSystemLocalDbContext dbContext) {
        _dbContext = dbContext;
    }
    public async Task<CachedUser?> FindByIdAsync(UserId userId, CancellationToken cancellationToken = default) {
        return await _dbContext.Users.FindAsync([userId], cancellationToken);
    }
    public async Task<IReadOnlyList<CachedUser>> FindByLoginOrTagAsync(string searchVal, int limit, int offset,
        CancellationToken cancellationToken = default) {

        var lowercaseSearchVal = searchVal.ToLower();

        return await _dbContext.Users
            .Where(u => u.Login.ToLower().Contains(lowercaseSearchVal) || u.Tag.ToLower().Contains(lowercaseSearchVal))
            .OrderBy(u => u.Login)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(CachedUser user, CancellationToken cancellationToken = default) {
        var existing = await _dbContext.Users.FindAsync([user.Id], cancellationToken);

        if (existing is null) {
            await _dbContext.Users.AddAsync(user, cancellationToken);
        }
        else {
            _dbContext.Entry(existing).CurrentValues.SetValues(user);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteAsync(UserId userId, CancellationToken cancellationToken = default) {
        await _dbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}