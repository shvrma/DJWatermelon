using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink;

internal readonly record struct LavalinkTrackHandle(
    string Encoded,
    LavalinkTrackInfo Info) : ITrackHandle
{
    public string Title { get => Info.Title; init => throw new NotSupportedException(); }
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
    int Duration,

    [property: JsonRequired]
    [property: JsonPropertyName("isStream")]
    bool IsStream,
    
    [property: JsonRequired]
    [property: JsonPropertyName("position")] 
    int CurrentPosition,

    [property: JsonRequired]
    [property: JsonPropertyName("title")]
    string Title,

    [property: JsonRequired]
    [property: JsonPropertyName("uri")]
    Uri? Uri,

    [property: JsonRequired]
    [property: JsonPropertyName("artworkUrl")]
    Uri? Artwork,

    [property: JsonRequired]
    [property: JsonPropertyName("isrc")]
    string ISRC,

    [property: JsonRequired]
    [property: JsonPropertyName("sourceName")]
    string Source);
