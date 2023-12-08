using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.EventPayloads;

internal sealed record TrackEndEventPayload(
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("reason")]
    TrackEndReason Reason) : EventPayload(GuildId);

public enum TrackEndReason : byte
{
    Finished,
    LoadFailed,
    Stopped,
    Replaced,
    Cleanup,
}
