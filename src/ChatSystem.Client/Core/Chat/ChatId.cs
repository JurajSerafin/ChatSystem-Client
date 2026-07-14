using System;
using ChatSystem.Client.Core.Interfaces;

namespace ChatSystem.Client.Core.Chat;

/// <summary>
/// Represents a strongly-typed, type-safe identifier for a Chat.
/// 
/// This is record struct that cannot be accidentally exchanged with other identifier types (like <see cref="Message.MessageId"/>)
/// at compile time.
/// </summary>
internal record struct ChatId(Guid Value) : IId<ChatId> {
    public static ChatId Create(Guid guid) => new(guid);
}