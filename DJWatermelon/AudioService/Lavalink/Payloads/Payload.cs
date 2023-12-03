using DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "op")]
[JsonDerivedType(typeof(ReadyPayload), "ready")]
[JsonDerivedType(typeof(PlayerUpdatePayload), "playerUpdate")]
[JsonDerivedType(typeof(EventPayload), "event")]
public record Payload
{
    [JsonRequired]
    [JsonPropertyName("op")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    OperationTypes OperationType { get; init; }
}

public enum OperationTypes
{
    Ready,
    PlayerUpdate,
    Stats,
    Event
}
