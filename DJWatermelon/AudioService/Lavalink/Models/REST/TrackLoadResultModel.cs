using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record TrackLoadResultModel(
    [property: JsonRequired]
    [property: JsonPropertyName("loadType")]
    [property: JsonConverter(typeof(JsonStringEnumConverter<LoadResultTypes>))]
    LoadResultTypes ResultType,

    [property: JsonRequired]
    [property: JsonPropertyName("data")]
    JsonNode Data);

public enum LoadResultTypes
{
    Track,
    Playlist,
    Search,
    Empty,
    Error
}

public interface ILoadResultData
{

}
