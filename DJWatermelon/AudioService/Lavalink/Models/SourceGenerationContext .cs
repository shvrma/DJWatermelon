using DJWatermelon.AudioService.Lavalink.Models;
using DJWatermelon.AudioService.Lavalink.Models.EventPayloads;
using System.Text.Json.Serialization;

namespace DJWatermelon;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(IPayload))]
[JsonSerializable(typeof(ReadyPayload))]
[JsonSerializable(typeof(PlayerUpdatePayload))]
[JsonSerializable(typeof(StatisticsPayload))]

[JsonSerializable(typeof(EventPayload))]
[JsonSerializable(typeof(TrackEndEventPayload))]
[JsonSerializable(typeof(TrackExceptionEventPayload))]
[JsonSerializable(typeof(TrackStartEventPayload))]
[JsonSerializable(typeof(TrackStuckEventPayload))]
[JsonSerializable(typeof(WebSocketClosedEventPayload))]
internal partial class PayloadsSourceGenerationContext : JsonSerializerContext
{
}
