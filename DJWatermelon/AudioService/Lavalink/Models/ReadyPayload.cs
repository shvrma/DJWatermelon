using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models;

public sealed record ReadyPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("resumed")]
    bool SessionResumed,

    [property: JsonRequired]
    [property: JsonPropertyName("sessionId")]
    string SessionId) : IPayload;