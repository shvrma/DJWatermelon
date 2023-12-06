using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

[JsonConverter(typeof(EventPayoadJsonConverter))]
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
    Unknown,
    TrackStartEvent,
    TrackEndEvent,
    TrackExceptionEvent,
    TrackStuckEvent,
    WebSocketClosedEvent
}
