﻿using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

internal sealed record class TrackStuckEventPayload(
    ulong GuildId,
    EventTypes EventType,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track,

    [property: JsonRequired]
    [property: JsonPropertyName("thresholdMs")]
    TimeSpan ExceededThreshold) : EventPayload(GuildId, EventType);
