﻿using Remora.Rest.Core;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;

public sealed record WebSocketClosedEventPayload(
    Snowflake GuildID,

    [property: JsonRequired]
    [property: JsonPropertyName("code")]
    int Code,

    [property: JsonRequired]
    [property: JsonPropertyName("reason")]
    string Reason,

    [property: JsonRequired]
    [property: JsonPropertyName("byRemote")]
    bool WasByRemote) : EventPayload(GuildID);
