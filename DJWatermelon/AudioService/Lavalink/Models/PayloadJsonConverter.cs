using DJWatermelon.AudioService.Lavalink.Models.EventPayloads;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Models;

internal class PayloadJsonConverter : JsonConverter<IPayload>
{
    public override IPayload? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                "Property name expected while reading through JSON.",
                path: string.Empty,
                lineNumber: -1,
                bytePositionInLine: reader.BytesConsumed);
        }

        Utf8JsonReader copyReader = reader;
        string? operationType = default;

        while (reader.Read())
        {
            // Go thru only on the highest level.
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (reader.CurrentDepth == 0)
                {
                    break;
                }
                else
                {
                    continue;
                }
            }

            // Get the key.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException(
                    "Property name expected while reading through JSON.",
                    path: string.Empty,
                    lineNumber: -1,
                    bytePositionInLine: reader.BytesConsumed);
            }
            string? propertyName = reader.GetString();
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new JsonException(
                    "Empty property name.",
                    path: string.Empty,
                    lineNumber: -1,
                    bytePositionInLine: reader.BytesConsumed);
            }

            // Check whatever the property is our type determinator.
            // Skip all the non-type-determinators properties.
            reader.Read();
            if (propertyName == "op")
            {
                operationType = reader.GetString();

                // Read till the end of the object.
                while (reader.Read())
                {

                }
                break;
            }
        }

        JsonSerializerOptions sourceGenOptions = new(options)
        {
            TypeInfoResolver = PayloadsSourceGenerationContext.Default
        };

        return operationType switch
        {
            "ready" => JsonSerializer.Deserialize<ReadyPayload>(
                ref copyReader,
                options: sourceGenOptions),

            "playerUpdate" => JsonSerializer.Deserialize<PlayerUpdatePayload>(
                ref copyReader,
                options: sourceGenOptions),

            "stats" => JsonSerializer.Deserialize<StatisticsPayload>(
                ref copyReader,
                options: sourceGenOptions),

            "event" => JsonSerializer.Deserialize<EventPayload>(
                ref copyReader,
                options: sourceGenOptions),

            _ => throw new InvalidOperationException("Unallowed value for OperationTypes.")
        };
    }

    public override void Write(Utf8JsonWriter writer, IPayload value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}
