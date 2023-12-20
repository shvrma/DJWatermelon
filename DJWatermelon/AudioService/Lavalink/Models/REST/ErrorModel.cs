using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record ErrorModel(
    [property: JsonRequired]
    [property: JsonPropertyName("timestamp")]
    int Timestamp,

    [property: JsonRequired]
    [property: JsonPropertyName("status")]
    short StatusCode,

    [property: JsonRequired]
    [property: JsonPropertyName("error")]
    string StatusCodeMessage,

    [property: JsonRequired]
    [property: JsonPropertyName("message")]
    string ErrorMessage,

    [property: JsonRequired]
    [property: JsonPropertyName("path")]
    string RequestPath
    /* TODO trace */) : RESTResponceModel;
