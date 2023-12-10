using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record PlaylistModel(
    [property: JsonRequired]
    [property: JsonPropertyName("info")]
    PlaylistInfoModel PlaylistInfo,

    [property: JsonRequired]
    [property: JsonPropertyName("tracks")]
    IEnumerable<LavalinkTrackHandle> Tracks);
