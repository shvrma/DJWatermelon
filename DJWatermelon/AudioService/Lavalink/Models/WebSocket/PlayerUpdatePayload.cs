﻿using Remora.Rest.Core;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket;

public sealed record PlayerUpdatePayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    Snowflake GuildID,

    [property: JsonRequired]
    [property: JsonPropertyName("state")]
    PlayerState State) : IPayload;