using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Interfaces.Ids;
using System;
using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Domain.Message;

/// <summary>
/// Represents a strongly-typed, type-safe identifier for a Message.
/// 
/// This is record struct that cannot be accidentally exchanged with other identifier types (like <see cref="User.UserId"/>)
/// at compile time.
/// </summary>
[JsonConverter(typeof(MessageIdJsonConverter))]
internal readonly record struct MessageId(Guid Value) : IId<MessageId> {
    public static MessageId Create(Guid guid) => new(guid);
}

/// <summary>
/// A concrete JSON converter for <see cref="MessageId"/> identifiers.
/// </summary>
internal sealed class MessageIdJsonConverter : DefaultIdJsonConverter<MessageId>;