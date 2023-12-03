﻿using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

public sealed record class ReadyPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("op")]
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    OperationTypes OperationType,

    [property: JsonRequired]
    [property: JsonPropertyName("resumed")]
    bool SessionResumed,

    [property: JsonRequired]
    [property: JsonPropertyName("sessionId")]
    string SessionId) : Payload;
