using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record PlayerUpdateModel(
    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    PlayerTrackUpateModel? TrackUpdate = default,

    [property: JsonRequired]
    [property: JsonPropertyName("position")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    int? PositionInTrack = default,

    [property: JsonRequired]
    [property: JsonPropertyName("endTime")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    int? TrackEndTime = default,

    [property: JsonRequired]
    [property: JsonPropertyName("volume")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    ushort? Volume = 100,

    [property: JsonRequired]
    [property: JsonPropertyName("paused")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    bool? IsPaused = false,

    [property: JsonRequired]
    [property: JsonPropertyName("filters")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    FiltersModel? Filters = default,

    [property: JsonRequired]
    [property: JsonPropertyName("voice")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    VoiceStateModel? VoiceState = default);
