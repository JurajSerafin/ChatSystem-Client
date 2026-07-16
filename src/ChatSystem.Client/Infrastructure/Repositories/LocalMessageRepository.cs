using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Interfaces.Database;
using ChatSystem.Client.Core.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Client.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of the local message repository. See <see cref="ILocalMessageRepository"/>
///
/// Interacts with the local database to store, search, and retrieve
/// fully decrypted E2EE chat messages for immediate UI rendering.
/// </summary>
internal class LocalMessageRepository : ILocalMessageRepository {

    private readonly ChatSystemLocalDbContext _dbContext;

    public LocalMessageRepository(ChatSystemLocalDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CachedMessage>> FindByChatIdAsync(ChatId chatId, int limit, int offset, CancellationToken cancellationToken = default) {
        return await _dbContext.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CachedMessage>> SearchAsync(ChatId chatId, string keywords, int limit,
        int offset, CancellationToken cancellationToken = default)
    {
        var lowercaseSearchVal = keywords.ToLower();

        return await _dbContext.Messages
            .Where(m => m.ChatId == chatId && m.PlainText.ToLower().Contains(lowercaseSearchVal))
            .OrderByDescending(m => m.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(MessageId id, CancellationToken cancellationToken = default) {
        await _dbContext.Messages
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(MessageId messageId, CancellationToken cancellationToken = default) {
        await _dbContext.Messages
            .Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(m => m.SetProperty(
                    x => x.IsRead, true),
                cancellationToken);
    }

    public async Task SaveForChatAsync(CachedMessage message, CancellationToken cancellationToken = default) {
        var existing = await _dbContext.Messages.FindAsync([message.Id], cancellationToken);

        if (existing is null) {
            await _dbContext.Messages.AddAsync(message, cancellationToken);
        } else {
            _dbContext.Entry(existing).CurrentValues.SetValues(message);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}