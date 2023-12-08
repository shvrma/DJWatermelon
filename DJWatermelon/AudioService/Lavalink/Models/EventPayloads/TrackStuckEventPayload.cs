using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.EventPayloads;

internal sealed record TrackStuckEventPayload(
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("thresholdMs")]
    int ExceededThreshold) : EventPayload(GuildId);
