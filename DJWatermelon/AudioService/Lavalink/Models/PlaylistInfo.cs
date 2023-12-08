using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Models;

public sealed record PlaylistInfo(
    [property: JsonRequired]
    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonRequired]
    [property: JsonPropertyName("selectedTrack")]
    int SelectedTrack);
