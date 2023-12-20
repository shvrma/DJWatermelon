using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record ErrorModel(
    [property: JsonRequired]
    [property: JsonPropertyName("timestamp")]
    ulong Timestamp,

    [property: JsonRequired]
    [property: JsonPropertyName("status")]
    ushort StatusCode,

    [property: JsonRequired]
    [property: JsonPropertyName("error")]
    string StatusCodeMessage,

    [property: JsonPropertyName("tace")]
    string? Trace,

    [property: JsonRequired]
    [property: JsonPropertyName("message")]
    string ErrorMessage,

    [property: JsonRequired]
    [property: JsonPropertyName("path")]
    string RequestPath) : IRESTResponceModel;
