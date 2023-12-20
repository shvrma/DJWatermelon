using DJWatermelon.AudioService.Lavalink.Models.REST;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models;

public sealed record class PlayerState(
    [property: JsonRequired]
    [property: JsonPropertyName("time")]
    ulong AbsoluteTimestamp,

    [property: JsonPropertyName("position")]
    ulong Position,

    [property: JsonRequired]
    [property: JsonPropertyName("connected")]
    bool IsConnected,

    [property: JsonRequired]
    [property: JsonPropertyName("ping")]
    int Latency) : RESTResponceModel, IPayload;
