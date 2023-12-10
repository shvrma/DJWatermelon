using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record VoiceStateModel(
    [property: JsonRequired]
    [property: JsonPropertyName("token")]
    string Token,

    [property: JsonRequired]
    [property: JsonPropertyName("endpoint")]
    string Endpoint,

    [property: JsonRequired]
    [property: JsonPropertyName("sessionId")]
    string SessionId);
