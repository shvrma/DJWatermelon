using DJWatermelon.AudioService.Lavalink.Models;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.EventPayloads;

[JsonConverter(typeof(EventPayoadJsonConverter))]
public record EventPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId) : IPayload;
