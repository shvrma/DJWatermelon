using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record PlayerTrackUpateModel(
    [property: JsonRequired]
    [property: JsonPropertyName("encoded")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    string Encoded);
