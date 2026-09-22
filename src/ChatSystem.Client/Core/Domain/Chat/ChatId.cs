using System;
using System.Text.Json.Serialization;
using ChatSystem.Client.Core.Interfaces.Ids;

namespace ChatSystem.Client.Core.Domain.Chat;

/// <summary>
/// Represents a strongly-typed, type-safe identifier for a Chat.
/// 
/// This is record struct that cannot be accidentally exchanged with other identifier types (like <see cref="Message.MessageId"/>)
/// at compile time.
/// </summary>
[JsonConverter(typeof(ChatIdJsonConverter))]
public readonly record struct ChatId(Guid Value) : IId<ChatId> {
    public static ChatId Create(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}

/// <summary>
/// A concrete JSON converter for <see cref="ChatId"/> identifiers.
/// </summary>
internal sealed class ChatIdJsonConverter : DefaultIdJsonConverter<ChatId>;