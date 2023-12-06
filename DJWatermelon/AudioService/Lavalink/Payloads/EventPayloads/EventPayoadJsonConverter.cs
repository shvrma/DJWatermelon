using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

internal class EventPayoadJsonConverter : JsonConverter<EventPayload>
{
    public override EventPayload? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("JSON reading started not from the very beginning.");
        }

        Utf8JsonReader copyReader = reader;
        EventTypes eventType = EventTypes.Unknown;
        while (reader.Read())
        {
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
            if (propertyName != "eventType")
            {
                // Skip all the non-type-determinators properties.
                reader.Read();
            }
            else
            {
                _ = Enum.TryParse(reader.GetString(), out eventType);
                break;
            }
        }


        return eventType switch
        {
            EventTypes.TrackStartEvent
                => JsonSerializer.Deserialize<TrackStartEventPayload>(ref copyReader, options),

            EventTypes.TrackEndEvent
                => JsonSerializer.Deserialize<TrackEndEventPayload>(ref copyReader, options),

            EventTypes.TrackStuckEvent
                => JsonSerializer.Deserialize<TrackStuckEventPayload>(ref copyReader, options),

            EventTypes.TrackExceptionEvent
                => JsonSerializer.Deserialize<TrackExceptionEventPayload>(ref copyReader, options),

            EventTypes.WebSocketClosedEvent
                => JsonSerializer.Deserialize<WebSocketClosedEventPayload>(ref copyReader, options),

            EventTypes.Unknown
                => default,

            _ => throw new InvalidOperationException("Unallowed value for EventTypes enum.")
        };
    }

    public override void Write(Utf8JsonWriter writer, EventPayload value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}
