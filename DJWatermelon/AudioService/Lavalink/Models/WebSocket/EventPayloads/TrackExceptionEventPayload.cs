using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;

public sealed record TrackExceptionEventPayload(
    Snowflake GuildID,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("exception")]
    LavalinkException Exception) : EventPayload(GuildID);

public sealed record LavalinkException(
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

public enum ExceptionSeverity : byte
{
    Common,
    Suspicious,
    Fatal,
    Fault,
}
