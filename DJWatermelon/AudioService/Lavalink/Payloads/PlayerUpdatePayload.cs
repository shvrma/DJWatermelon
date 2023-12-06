using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

public sealed record class PlayerUpdatePayload(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuildId,

    [property: JsonRequired]
    [property: JsonPropertyName("state")]
    PlayerStateModel State) : IPayload
{
    public OperationTypes OperationType { get; init; }
}

public sealed record class PlayerStateModel(
    [property: JsonRequired]
    [property: JsonPropertyName("time")]
    DateTimeOffset AbsoluteTimestamp,

    [property: JsonPropertyName("position")]
    TimeSpan Position,

    [property: JsonRequired]
    [property: JsonPropertyName("connected")]
    bool IsConnected,

    [property: JsonRequired]
    [property: JsonPropertyName("ping")]
    TimeSpan? Latency);
