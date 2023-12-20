using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record PlaylistInfoModel(
    [property: JsonRequired]
    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonRequired]
    [property: JsonPropertyName("selectedTrack")]
    short SelectedTrack) : IRESTResponceModel;
