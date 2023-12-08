using DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

[JsonConverter(typeof(PayloadJsonConverter))]
public interface IPayload
{

}

public enum OperationTypes
{
    Unknown,
    Ready,
    PlayerUpdate,
    Stats,
    Event
}
