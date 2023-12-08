using DJWatermelon.AudioService.Lavalink.Models.EventPayloads;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models;

[JsonConverter(typeof(PayloadJsonConverter))]
public interface IPayload
{

}
