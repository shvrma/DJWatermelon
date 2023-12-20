using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record FiltersModel(
    [property: JsonRequired]
    [property: JsonPropertyName("volume")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Volume,

    [property: JsonRequired]
    [property: JsonPropertyName("equalizer")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    IEnumerable<Equalizer>? Equalizers,

    [property: JsonRequired]
    [property: JsonPropertyName("karaoke")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Karaoke? Karaoke,

    [property: JsonRequired]
    [property: JsonPropertyName("timescale")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Timescale? Timescale,

    [property: JsonRequired]
    [property: JsonPropertyName("tremolo")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Tremolo? Tremolo,

    [property: JsonRequired]
    [property: JsonPropertyName("vibrato")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Vibrato? Vibrato,

    [property: JsonRequired]
    [property: JsonPropertyName("rotation")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Rotation? Rotation,

    [property: JsonRequired]
    [property: JsonPropertyName("distortion")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Distortion? Distortion,

    [property: JsonRequired]
    [property: JsonPropertyName("channelMix")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    ChannelMix? ChannelMix,

    [property: JsonRequired]
    [property: JsonPropertyName("lowPass")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    LowPass? LowPass,

    [property: JsonRequired]
    [property: JsonPropertyName("pluginFilters")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    IDictionary<string, object>? PluginFilters);

public sealed record Equalizer(
    [property: JsonRequired]
    [property: JsonPropertyName("band")]
    byte Band,

    [property: JsonRequired]
    [property: JsonPropertyName("gain")]
    float Gain);

public sealed record Karaoke(
    [property: JsonRequired]
    [property: JsonPropertyName("level")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Level,

    [property: JsonRequired]
    [property: JsonPropertyName("monoLevel")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? MonoLevel,

    [property: JsonRequired]
    [property: JsonPropertyName("filterBand")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? FilterBand,

    [property: JsonRequired]
    [property: JsonPropertyName("filterWidth")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? FilterWidth);

public sealed record Timescale(
    [property: JsonRequired]
    [property: JsonPropertyName("spped")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Speed,

    [property: JsonRequired]
    [property: JsonPropertyName("pitch")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Pitch,

    [property: JsonRequired]
    [property: JsonPropertyName("rate")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Rate);

public sealed record Tremolo(
    [property: JsonRequired]
    [property: JsonPropertyName("frequency")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Frequency,

    [property: JsonRequired]
    [property: JsonPropertyName("depth")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Depth);

public sealed record Vibrato(
    [property: JsonRequired]
    [property: JsonPropertyName("frequency")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Frequency,

    [property: JsonRequired]
    [property: JsonPropertyName("depth")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Depth);

public sealed record Rotation(
    [property: JsonRequired]
    [property: JsonPropertyName("rotationHz")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? RotationFrequency);

public sealed record Distortion(
    [property: JsonRequired]
    [property: JsonPropertyName("sinOffset")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? SinOffset,

    [property: JsonRequired]
    [property: JsonPropertyName("sinScale")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? SinScale,

    [property: JsonRequired]
    [property: JsonPropertyName("cosOffset")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? CosOffset,

    [property: JsonRequired]
    [property: JsonPropertyName("sosScale")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? CosScale,

    [property: JsonRequired]
    [property: JsonPropertyName("tanOffset")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? TanOffset,

    [property: JsonRequired]
    [property: JsonPropertyName("tanScale")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? TanScale,

    [property: JsonRequired]
    [property: JsonPropertyName("offset")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Offset,

    [property: JsonRequired]
    [property: JsonPropertyName("scale")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Scale);

public sealed record ChannelMix(
    [property: JsonRequired]
    [property: JsonPropertyName("leftToLeft")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? LeftToLeft,

    [property: JsonRequired]
    [property: JsonPropertyName("leftToRight")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? LeftToRight,

    [property: JsonRequired]
    [property: JsonPropertyName("rightToLeft")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? RightToLeft,

    [property: JsonRequired]
    [property: JsonPropertyName("rightToRight")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? RightToRight);

public sealed record LowPass(
    [property: JsonRequired]
    [property: JsonPropertyName("smoothing")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    float? Smoothing);
