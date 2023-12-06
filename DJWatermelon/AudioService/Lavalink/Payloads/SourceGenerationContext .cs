using DJWatermelon.AudioService.Lavalink.Payloads;
using DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;
using System.Text.Json.Serialization;

namespace DJWatermelon;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IPayload))]
[JsonSerializable(typeof(PlayerUpdatePayload))]
[JsonSerializable(typeof(ReadyPayload))]

[JsonSerializable(typeof(EventPayload))]
[JsonSerializable(typeof(TrackEndEventPayload))]
[JsonSerializable(typeof(TrackExceptionEventPayload))]
[JsonSerializable(typeof(TrackStartEventPayload))]
[JsonSerializable(typeof(TrackStuckEventPayload))]
[JsonSerializable(typeof(WebSocketClosedEventPayload))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
