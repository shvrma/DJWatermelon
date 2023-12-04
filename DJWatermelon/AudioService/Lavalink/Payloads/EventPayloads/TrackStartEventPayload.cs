﻿using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

internal sealed record class TrackStartEventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle Track) : EventPayload;
