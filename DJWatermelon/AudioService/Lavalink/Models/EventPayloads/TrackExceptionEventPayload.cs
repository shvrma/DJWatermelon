using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.EventPayloads;

internal sealed record TrackExceptionEventPayload(
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("exception")]
    TrackExceptionModel Exception) : EventPayload(GuildId);

internal sealed record TrackExceptionModel(
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
