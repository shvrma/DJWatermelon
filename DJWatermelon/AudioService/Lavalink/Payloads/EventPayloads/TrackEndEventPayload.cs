using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

internal sealed record class TrackEndEventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("reason")]
    TrackEndReason Reason) : IEventPayload;

public enum TrackEndReason : byte
{
    Finished,
    LoadFailed,
    Stopped,
    Replaced,
    Cleanup,
}
