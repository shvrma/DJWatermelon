using Remora.Rest.Core;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;

public sealed record TrackExceptionEventPayload(
    Snowflake GuildID,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("exception")]
    LavalinkExceptionModel Exception) : EventPayload(GuildID);
