using System;
using ChatSystem.Client.Core.Interfaces.Ids;

namespace ChatSystem.Client.Core.Domain.Message;

/// <summary>
/// Represents a strongly-typed, type-safe identifier for a Message.
/// 
/// This is record struct that cannot be accidentally exchanged with other identifier types (like <see cref="User.UserId"/>)
/// at compile time.
/// </summary>
internal readonly record struct MessageId(Guid Value) : IId<MessageId> {
    public static MessageId Create(Guid guid) => new(guid);
}