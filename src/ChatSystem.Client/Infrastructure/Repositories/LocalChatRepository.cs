using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Database;
using ChatSystem.Client.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Client.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of the local chat repository. See <see cref="ILocalChatRepository"/>.
///
/// Acts as a domain-specific wrapper that delegates chat and participant
/// storage operations to the underlying SQLite database engine.
/// </summary>
internal class LocalChatRepository : ILocalChatRepository{

    private readonly ChatSystemLocalDbContext _dbContext;

    public LocalChatRepository(ChatSystemLocalDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CachedChat>> FindAllAsync(int limit, int offset, CancellationToken cancellationToken = default) {
        return await _dbContext.Chats
            .OrderByDescending(c => c.LastActivityAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
    public async Task<CachedChat?> FindByIdAsync(ChatId id, CancellationToken cancellationToken = default) {
        return await _dbContext.Chats.FindAsync([id], cancellationToken);
    }
    public async Task UpsertAsync(CachedChat chat, CancellationToken cancellationToken = default) {
        try {
            var existing = await _dbContext.Chats.FindAsync([chat.Id], cancellationToken);

            if (existing is null) {
                await _dbContext.Chats.AddAsync(chat, cancellationToken);
            } else {
                _dbContext.Entry(existing).CurrentValues.SetValues(chat);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException) {
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task AddParticipantAsync(UserId userId, ChatId chatId, string role, CancellationToken cancellationToken = default) {
        try {
            await _dbContext.ChatParticipants.AddAsync(new CachedChatParticipant {
                ChatId = chatId,
                UserId = userId,
                Role = role
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException) {
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task DeleteAsync(ChatId chatId, CancellationToken cancellationToken = default) {
        await _dbContext.Chats
            .Where(c => c.Id == chatId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserId>> GetParticipantIdsAsync(ChatId chatId,
        CancellationToken cancellationToken = default) {
        return await _dbContext.ChatParticipants
            .Where(cp => cp.ChatId == chatId)
            .Select(cp => cp.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveParticipantAsync(UserId userId, ChatId chatId, CancellationToken cancellationToken = default) {
        await _dbContext.ChatParticipants
            .Where(cp => cp.UserId == userId && cp.ChatId == chatId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}