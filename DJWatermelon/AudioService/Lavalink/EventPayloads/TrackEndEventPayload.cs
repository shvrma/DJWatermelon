using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.EventPayloads;

public sealed record class TrackEndEventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    TrackModel Track,

    [property: JsonRequired]
    [property: JsonPropertyName("reason")]
    TrackEndReason Reason) : IEventPayload;
