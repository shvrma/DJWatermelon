using Remora.Rest.Core;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;

public sealed record TrackStuckEventPayload(
    Snowflake GuildID,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("thresholdMs")]
    int ExceededThreshold) : EventPayload(GuildID);
