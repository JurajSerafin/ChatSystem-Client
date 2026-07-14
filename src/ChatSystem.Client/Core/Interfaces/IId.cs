using System;

namespace ChatSystem.Client.Core.Interfaces;

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
    Guid Value { get; set; }

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

/// <summary>
/// Provides a generic factory for creating, generating, and parsing 
/// strongly-typed identifiers conforming to <see cref="IId{TSelf}"/>.
/// </summary>
internal static class IdFactory {

    /// <summary>
    /// Generates a new unique identifier of type <typeparamref name="TId"/>.
    /// </summary>
    /// <typeparam name="TId">The type of identifier to generate.</typeparam>
    /// <returns>A newly generated identifier.</returns>
    public static TId Generate<TId>() where TId : struct, IId<TId> {
        return TId.Generate();
    }

    /// <summary>
    /// Parses a raw string into an identifier of type <typeparamref name="TId"/>.
    /// </summary>
    /// <typeparam name="TId">The target type of the identifier.</typeparam>
    /// <param name="idString">The string representation of the identifier.</param>
    /// <returns>The parsed identifier.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="idString"/> is not in a valid format.</exception>
    public static TId Parse<TId>(string idString) where TId : struct, IId<TId> {
        return TId.Parse(idString);
    }

    /// <summary>
    /// Wraps an existing raw <see cref="Guid"/> into a strongly-typed identifier of type <typeparamref name="TId"/>.
    /// </summary>
    /// <typeparam name="TId">The target type of the identifier.</typeparam>
    /// <param name="guid">The raw GUID value to wrap.</param>
    /// <returns>A strongly-typed identifier wrapping the provided GUID.</returns>
    public static TId Create<TId>(Guid guid) where TId : struct, IId<TId> {
        return TId.Create(guid);
    }
}