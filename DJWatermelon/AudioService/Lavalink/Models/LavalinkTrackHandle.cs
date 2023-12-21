using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models;

public record struct LavalinkTrackHandle(
    [property: JsonRequired]
    [property: JsonPropertyName("encoded")]
    string Encoded,

    [property: JsonRequired]
    [property: JsonPropertyName("info")]
    LavalinkTrackInfo Info,

    [property: JsonRequired]
    [property: JsonPropertyName("pluginInfo")]
    [property: JsonInclude]
    object? PluginInfo = default,

    [property: JsonRequired]
    [property: JsonPropertyName("userData")]
    [property: JsonInclude]
    object? UserData = default) : ITrackHandle
{
    public string Title { readonly get => Info.Title; init => throw new NotSupportedException(); }
}

public readonly record struct LavalinkTrackInfo(
    [property: JsonRequired]
    [property: JsonPropertyName("identifier")]
    string Id,

    [property: JsonRequired]
    [property: JsonPropertyName("isSeekable")]
    bool IsSeekable,

    [property: JsonRequired]
    [property: JsonPropertyName("author")]
    string Author,

    [property: JsonRequired]
    [property: JsonPropertyName("length")]
    ulong Duration,

    [property: JsonRequired]
    [property: JsonPropertyName("isStream")]
    bool IsStream,

    [property: JsonRequired]
    [property: JsonPropertyName("position")]
    ulong CurrentPosition,

    [property: JsonRequired]
    [property: JsonPropertyName("title")]
    string Title,

    [property: JsonRequired]
    [property: JsonPropertyName("uri")]
    [property: JsonInclude]
    Uri? Uri,

    [property: JsonRequired]
    [property: JsonPropertyName("artworkUrl")]
    [property: JsonInclude]
    Uri? Artwork,

    [property: JsonRequired]
    [property: JsonPropertyName("isrc")]
    [property: JsonInclude]
    string? ISRC,

    [property: JsonRequired]
    [property: JsonPropertyName("sourceName")]
    string Source);
