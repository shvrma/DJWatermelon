using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

[JsonConverter(typeof(EventPayoadJsonConverter))]
public record EventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId) : IPayload;
