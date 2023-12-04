using DJWatermelon.AudioService.Lavalink.Payloads;
using DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace DJWatermelon;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Payload))]
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
