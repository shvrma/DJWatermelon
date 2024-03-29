﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;

public sealed class EventPayoadJsonConverter : JsonConverter<EventPayload>
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
        string? eventType = default;

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
                eventType = reader.GetString();

                // Read till the end of the object.
                while (reader.Read())
                {

                }
                break;
            }
        }

        return eventType switch
        {
            "TrackStartEvent"
                => JsonSerializer.Deserialize<TrackStartEventPayload>(
                    ref copyReader,
                    options),

            "TrackEndEvent"
                => JsonSerializer.Deserialize<TrackEndEventPayload>(
                    ref copyReader,
                    options),

            "TrackStuckEvent"
                => JsonSerializer.Deserialize<TrackStuckEventPayload>(
                    ref copyReader,
                    options),

            "TrackExceptionEvent"
                => JsonSerializer.Deserialize<TrackExceptionEventPayload>(
                    ref copyReader,
                    options),

            "WebSocketClosedEvent"
                => JsonSerializer.Deserialize<WebSocketClosedEventPayload>(
                    ref copyReader,
                    options),

            _ => throw new InvalidOperationException("Unallowed value for EventType.")
        };
    }

    public override void Write(Utf8JsonWriter writer, EventPayload value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}
