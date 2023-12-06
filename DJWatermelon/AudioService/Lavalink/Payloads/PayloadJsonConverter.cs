using DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

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
        OperationTypes operationType = OperationTypes.Unknown;
        while (reader.Read())
        {
            // Get the key.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
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
            if (propertyName != "op")
            {
                // Skip all the non-type-determinators properties.
                reader.Read();
            }
            else
            {
                _ = Enum.TryParse(reader.GetString(), out operationType);
                break;
            }
        }


        return operationType switch
        {
            OperationTypes.Ready 
                => JsonSerializer.Deserialize<ReadyPayload>(ref copyReader, options),

            OperationTypes.PlayerUpdate 
                => JsonSerializer.Deserialize<PlayerUpdatePayload>(ref copyReader, options),

            OperationTypes.Event 
                => JsonSerializer.Deserialize<EventPayload>(ref copyReader, options),

            OperationTypes.Stats
                => default,

            OperationTypes.Unknown
                => default,

            _ => throw new InvalidOperationException("Unallowed value for Operation Types enum.")
        };
    }

    public override void Write(Utf8JsonWriter writer, IPayload value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}
