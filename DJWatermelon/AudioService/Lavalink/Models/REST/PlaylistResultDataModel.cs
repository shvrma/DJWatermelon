using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record PlaylistResultDataModel(
    [property: JsonRequired]
    [property: JsonPropertyName("info")]
    PlaylistInfoModel PlaylistInfo,

    [property: JsonRequired]
    [property: JsonPropertyName("tracks")]
    IEnumerable<LavalinkTrackHandle> Tracks,

    [property: JsonRequired]
    [property: JsonPropertyName("tracks")]
    [property: JsonInclude]
    object? PluginInfo = default);
