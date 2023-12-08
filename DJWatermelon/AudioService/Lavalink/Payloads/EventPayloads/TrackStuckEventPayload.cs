using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

internal sealed record class TrackStuckEventPayload(
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("thresholdMs")]
    int ExceededThreshold) : EventPayload(GuildId);
