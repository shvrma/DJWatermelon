using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.EventPayloads;

internal sealed record TrackStartEventPayload(
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track) : EventPayload(GuildId);
