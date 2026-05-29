using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodingCell.YARPad;

public sealed record DestinationModel
{
 public bool IsEnabled { get; set; } = true;

    public required string ID { get; set; }

    public required string Address { get; set; }

    public string? Health { get; set; }

    [JsonConverter(typeof(MetadataConverter))]
    public List<YarpMetadata> Metadata { get; set; } = [];

    public string? Host { get; set; }
}

//TODO: Remove on next release
public class MetadataConverter : JsonConverter<List<YarpMetadata>>
{
    public override List<YarpMetadata> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Old dictionary format — just skip it and return empty list
            using var doc = JsonDocument.ParseValue(ref reader);
            return new List<YarpMetadata>();
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return JsonSerializer.Deserialize<List<YarpMetadata>>(ref reader, options)
                   ?? new List<YarpMetadata>();
        }

        return new List<YarpMetadata>();
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<YarpMetadata> value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}