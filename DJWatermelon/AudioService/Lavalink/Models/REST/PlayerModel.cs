using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record PlayerModel(
    [property: JsonRequired]
    [property: JsonPropertyName("guildId")]
    ulong GuilId,

    [property: JsonRequired]
    [property: JsonPropertyName("track")]
    LavalinkTrackHandle? CurrentTrack,

    [property: JsonRequired]
    [property: JsonPropertyName("volume")]
    short Volume,

    [property: JsonRequired]
    [property: JsonPropertyName("paused")]
    bool IsPaused,

    [property: JsonRequired]
    [property: JsonPropertyName("state")]
    PlayerState PlayerState,

    [property: JsonRequired]
    [property: JsonPropertyName("voice")]
    VoiceStateModel VoiceState
    /* TODO, Filters */);
