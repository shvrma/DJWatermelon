using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record PlayerUpdateModel(
    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    PlayerTrackUpateModel? TrackUpdate,

    [property: JsonRequired]
    [property: JsonPropertyName("voice")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    VoiceStateModel? VoiceState,

    [property: JsonRequired]
    [property: JsonPropertyName("endTime")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    int? TrackEndTime = default,

    [property: JsonRequired]
    [property: JsonPropertyName("position")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    int PositionInTrack = 0,

    [property: JsonRequired]
    [property: JsonPropertyName("volume")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    int? Volume = 100,

    [property: JsonRequired]
    [property: JsonPropertyName("paused")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    bool? IsPaused = false

    /* TODO filters */);
