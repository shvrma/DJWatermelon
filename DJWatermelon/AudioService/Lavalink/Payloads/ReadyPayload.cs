using DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

public sealed record class ReadyPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("resumed")]
    bool SessionResumed,

    [property: JsonRequired]
    [property: JsonPropertyName("sessionId")]
    string SessionId) : Payload;
