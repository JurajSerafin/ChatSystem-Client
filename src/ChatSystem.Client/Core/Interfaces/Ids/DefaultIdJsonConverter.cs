using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatSystem.Client.Core.Interfaces.Ids;

/// <summary>
/// Provides a base, reusable <see cref="JsonConverter{T}"/> implementation for strongly-typed 
/// identifiers conforming to <see cref="IId{TSelf}"/>.
/// 
/// This converter serializes the identifier directly to its underlying raw value (e.g., a string representation of its GUID) 
/// and deserializes it back into the strongly-typed wrapper, ensuring clean API payloads.
/// </summary>
/// <typeparam name="TId">The type of the strongly-typed identifier.</typeparam>
internal abstract class DefaultIdJsonConverter<TId> : JsonConverter<TId> where TId : struct, IId<TId> {

    /// <summary>
    /// Reads and converts a JSON string token into a strongly-typed identifier of type <typeparamref name="TId"/>.
    /// </summary>
    /// <param name="reader">The reader to extract the JSON token from.</param>
    /// <param name="typeToConvert">The target type being converted.</param>
    /// <param name="options">Serializer options to apply during deserialization.</param>
    /// <returns>A fully parsed, strongly-typed identifier instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the deserialized token is null or cannot be read as a string.</exception>
    /// <exception cref="FormatException">Thrown when the read string is not in a valid GUID format.</exception>
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return FromReadStrParseIdOrThrow(reader.GetString(), $"Token representing a {nameof(TId)} is null.");
    }

    /// <summary>
    /// Writes the underlying raw <see cref="Guid"/> value of the identifier as a JSON string token.
    /// </summary>
    /// <param name="writer">The writer to write the JSON token to.</param>
    /// <param name="value">The strongly-typed identifier to serialize.</param>
    /// <param name="options">Serializer options to apply during serialization.</param>
    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.Value);
    }

    /// <summary>
    /// Reads and converts a JSON property name string into a strongly-typed identifier of type <typeparamref name="TId"/>.
    /// </summary>
    /// <param name="reader">The reader to extract the JSON property name token from.</param>
    /// <param name="typeToConvert">The target type being converted.</param>
    /// <param name="options">Serializer options to apply during deserialization.</param>
    /// <returns>A fully parsed, strongly-typed identifier instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the property name token is null or cannot be read as a string.</exception>
    /// <exception cref="FormatException">Thrown when the read string is not in a valid GUID format.</exception>
    public override TId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return FromReadStrParseIdOrThrow(reader.GetString(), $"Property name representing a {nameof(TId)} is null.");
    }

    /// <summary>
    /// Writes the string representation of the strongly-typed identifier as a JSON property name.
    /// </summary>
    /// <param name="writer">The writer to write the JSON property name to.</param>
    /// <param name="value">The strongly-typed identifier to serialize as a property name.</param>
    /// <param name="options">Serializer options to apply during serialization.</param>
    /// <exception cref="InvalidOperationException">Thrown when the string representation of the identifier is null or empty.</exception>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, TId value, JsonSerializerOptions options) {
        var valueStr = value.ToString();

        if (string.IsNullOrEmpty(valueStr)) {
            throw new InvalidOperationException($"The string representation of {nameof(TId)} cannot be null or empty.");
        }

        writer.WritePropertyName(valueStr);
    }

    private TId FromReadStrParseIdOrThrow(string? readStr, string errMessage) {
        return readStr is null
            ? throw new ArgumentNullException(readStr, errMessage)
            : IdFactory.Parse<TId>(readStr);
    }
}