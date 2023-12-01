using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.EventPayloads;

public sealed record class TrackStuckEventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    TrackModel Track,

    [property: JsonRequired]
    [property: JsonPropertyName("thresholdMs")]
    [property: JsonConverter(typeof(DurationJsonConverter))]
    TimeSpan ExceededThreshold) : IEventPayload;
