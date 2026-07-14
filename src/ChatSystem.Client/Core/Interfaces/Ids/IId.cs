using System;

namespace ChatSystem.Client.Core.Interfaces.Ids;

/// <summary>
/// Defines the contract for strongly-typed, type-safe identifiers.
/// 
/// Implementing this interface creates a distinct ID type (e.g., UserId vs ChatId),
/// which prevents accidental mixing of different identifier types at compile time.
/// 
/// This interface leverages static abstract and static virtual members (C# 11+) 
/// to enforce unified creation, generation, and parsing APIs on implementing types.
/// </summary>
/// <typeparam name="TSelf">The concrete struct type implementing this interface.</typeparam>
internal interface IId<TSelf> where TSelf : struct, IId<TSelf> {

    /// <summary>
    /// Gets or sets the underlying raw <see cref="Guid"/> value.
    /// </summary>
    Guid Value { get; init; }

    /// <summary>
    /// Creates a new instance of the strongly-typed identifier with a specified <see cref="Guid"/>.
    /// </summary>
    /// <param name="guid">The raw unique identifier value.</param>
    /// <returns>An instance of the concrete identifier type <typeparamref name="TSelf"/>.</returns>
    static abstract TSelf Create(Guid guid);

    /// <summary>
    /// Parses a string representation of a GUID into the strongly-typed identifier.
    /// </summary>
    /// <param name="idString">The string representation of the GUID to parse.</param>
    /// <returns>An instance of the concrete identifier type <typeparamref name="TSelf"/>.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="idString"/> is not a valid GUID format.</exception>
    static virtual TSelf Parse(string idString) {
        return Guid.TryParse(idString, out var guid)
            ? TSelf.Create(guid)
            : throw new FormatException($"Invalid GUID: {idString}");
    }

    /// <summary>
    /// Generates a new, cryptographically secure random identifier.
    /// </summary>
    /// <returns>A newly generated instance of the concrete identifier type <typeparamref name="TSelf"/>.</returns>
    static virtual TSelf Generate() {
        return TSelf.Create(Guid.NewGuid());
    }
}