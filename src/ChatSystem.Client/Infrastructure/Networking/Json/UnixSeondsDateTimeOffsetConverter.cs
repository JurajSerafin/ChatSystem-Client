using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatSystem.Client.Infrastructure.Networking.Json;

/// <summary>
/// Converts DateTimeOffset to/from Unix epoch seconds (a raw JSON number),
/// matching the C++ server's std::chrono::seconds serialization.
/// </summary>
internal sealed class UnixSecondsDateTimeOffsetConverter : JsonConverter<DateTimeOffset> {
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var epochSeconds = reader.GetInt64();
        return DateTimeOffset.FromUnixTimeSeconds(epochSeconds);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}