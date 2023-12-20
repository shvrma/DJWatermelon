using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record PlaylistModel(
    [property: JsonRequired]
    [property: JsonPropertyName("info")]
    PlaylistInfoModel PlaylistInfo,

    [property: JsonRequired]
    [property: JsonPropertyName("tracks")]
    IEnumerable<LavalinkTrackHandle> Tracks);
