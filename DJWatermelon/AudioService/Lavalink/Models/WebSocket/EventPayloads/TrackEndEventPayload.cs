using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;

public sealed record TrackEndEventPayload(
    Snowflake GuildID,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("reason")]
    TrackEndReason Reason) : EventPayload(GuildID);

public enum TrackEndReason : byte
{
    Finished,
    LoadFailed,
    Stopped,
    Replaced,
    Cleanup,
}
