using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket;

[JsonConverter(typeof(PayloadJsonConverter))]
public interface IPayload
{

}
