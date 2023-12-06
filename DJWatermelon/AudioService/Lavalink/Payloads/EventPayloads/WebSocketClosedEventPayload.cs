using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

internal sealed record class WebSocketClosedEventPayload(
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("code")]
    int Code,

    [property: JsonRequired]
    [property: JsonPropertyName("reason")]
    string Reason,

    [property: JsonRequired]
    [property: JsonPropertyName("byRemote")]
    bool WasByRemote) : EventPayload(GuildId);
