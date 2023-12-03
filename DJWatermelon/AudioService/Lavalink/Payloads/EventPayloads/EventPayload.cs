using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TrackEndEventPayload), "TrackEndEvent")]
[JsonDerivedType(typeof(TrackExceptionEventPayload), "TrackExceptionEvent")]
[JsonDerivedType(typeof(TrackStartEventPayload), "TrackStartEvent")]
[JsonDerivedType(typeof(TrackStuckEventPayload), "TrackStuckEvent")]
[JsonDerivedType(typeof(WebSocketClosedEventPayload), "WebSocketClosedEvent")]
public record EventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("type")]
    [property: JsonConverter(typeof(StringEnumConverter))]
    EventTypes EventType
): Payload;

public enum EventTypes
{
    TrackStartEvent, 
    TrackEndEvent, 
    TrackExceptionEvent, 
    TrackStuckEvent, 
    WebSocketClosedEvent
}
