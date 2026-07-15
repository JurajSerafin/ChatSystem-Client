using System;
using System.Text.Json.Serialization;
using ChatSystem.Client.Core.Interfaces.Ids;

namespace ChatSystem.Client.Core.Domain.User;

/// <summary>
/// Represents a strongly-typed, type-safe identifier for a User.
/// 
/// This is record struct that cannot be accidentally exchanged with other identifier types (like <see cref="Message.MessageId"/>)
/// at compile time.
/// </summary>
[JsonConverter(typeof(UserIdJsonConverter))]
internal readonly record struct UserId(Guid Value) : IId<UserId> {
    public static UserId Create(Guid guid) => new(guid);
}

/// <summary>
/// A concrete JSON converter for <see cref="UserId"/> identifiers.
/// </summary>
internal sealed class UserIdJsonConverter : DefaultIdJsonConverter<UserId>;