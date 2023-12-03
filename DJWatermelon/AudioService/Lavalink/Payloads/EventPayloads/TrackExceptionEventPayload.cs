using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

internal sealed record class TrackExceptionEventPayload(
    ulong GuildId,
    EventTypes EventType,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("exception")]
    TrackExceptionModel Exception) : EventPayload(GuildId, EventType);

internal sealed record class TrackExceptionModel(
    [property: JsonRequired]
    [property: JsonPropertyName("message")]
    string Message,

    [property: JsonRequired]
    [property: JsonPropertyName("severity")]
    ExceptionSeverity Severity,

    [property: JsonRequired]
    [property: JsonPropertyName("cause")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Cause);

internal enum ExceptionSeverity : byte
{
    Common,
    Suspicious,
    Fatal,
    Fault,
}
