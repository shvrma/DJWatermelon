using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models;

public sealed record PlayerUpdatePayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("state")]
    PlayerStateModel State) : IPayload;

public sealed record class PlayerStateModel(
    [property: JsonRequired]
    [property: JsonPropertyName("time")]
    int AbsoluteTimestamp,

    [property: JsonPropertyName("position")]
    int Position,

    [property: JsonRequired]
    [property: JsonPropertyName("connected")]
    bool IsConnected,

    [property: JsonRequired]
    [property: JsonPropertyName("ping")]
    int Latency);
