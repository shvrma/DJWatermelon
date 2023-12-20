using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record TrackLoadResultModel(
    [property: JsonRequired]
    [property: JsonPropertyName("loadType")]
    LoadResultTypes ResultType,

    [property: JsonRequired]
    [property: JsonPropertyName("data")]
    JsonNode Data);

[JsonConverter(typeof(JsonStringEnumConverter<LoadResultTypes>))]
public enum LoadResultTypes
{
    Track,
    Playlist,
    Search,
    Empty,
    Error
}
