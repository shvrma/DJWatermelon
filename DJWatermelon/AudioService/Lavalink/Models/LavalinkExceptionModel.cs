using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Models;

public sealed record LavalinkExceptionModel(
    [property: JsonRequired]
    [property: JsonPropertyName("message")]
    [property: JsonInclude]
    string? Message,

    [property: JsonRequired]
    [property: JsonPropertyName("severity")]
    ExceptionSeverity Severity,

    [property: JsonRequired]
    [property: JsonPropertyName("cause")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Cause);

[JsonConverter(typeof(JsonStringEnumConverter<ExceptionSeverity>))]
public enum ExceptionSeverity : byte
{
    Common,
    Suspicious,
    Fatal,
    Fault,
}
