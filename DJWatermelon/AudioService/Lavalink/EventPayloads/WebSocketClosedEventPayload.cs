using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.EventPayloads;

public sealed record class WebSocketClosedEventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("code")]
    int Code,

    [property: JsonRequired]
    [property: JsonPropertyName("reason")]
    string Reason,

    [property: JsonRequired]
    [property: JsonPropertyName("byRemote")]
    bool WasByRemote) : IEventPayload;