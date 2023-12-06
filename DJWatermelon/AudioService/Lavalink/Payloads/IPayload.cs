using DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

[JsonConverter(typeof(PayloadJsonConverter))]
public interface IPayload
{
    [JsonRequired]
    [JsonPropertyName("op")]
    [JsonConverter(typeof(JsonStringEnumConverter<OperationTypes>))]
    public OperationTypes OperationType { get; init; }
}

public enum OperationTypes
{
    Unknown,
    Ready,
    PlayerUpdate,
    Stats,
    Event
}
