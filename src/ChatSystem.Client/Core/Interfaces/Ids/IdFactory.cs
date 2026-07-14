using System;

namespace ChatSystem.Client.Core.Interfaces.Ids;

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