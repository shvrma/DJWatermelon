﻿using Remora.Rest.Core;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;

[JsonConverter(typeof(EventPayoadJsonConverter))]
public record EventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    Snowflake GuildID) : IWebSocketPayload;
