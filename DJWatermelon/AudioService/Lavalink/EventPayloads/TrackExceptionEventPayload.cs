using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.EventPayloads;

internal sealed record class TrackExceptionEventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("exception")]
    TrackExceptionModel Exception) : IEventPayload;

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
