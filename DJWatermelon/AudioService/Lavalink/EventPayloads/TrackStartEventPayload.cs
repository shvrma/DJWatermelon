using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.EventPayloads;

internal sealed record class TrackStartEventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track) : IEventPayload;
