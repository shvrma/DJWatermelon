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
    ulong GuildId) : IPayload
{
    [JsonRequired]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<EventTypes>))]
    public EventTypes EventType { get; init; }
    public OperationTypes OperationType { get; init; }
}

public enum EventTypes
{
    TrackStartEvent,
    TrackEndEvent,
    TrackExceptionEvent,
    TrackStuckEvent,
    WebSocketClosedEvent
}
