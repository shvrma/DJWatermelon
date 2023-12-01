using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.EventPayloads;

public enum TrackEndReason : byte
{
    Finished,
    LoadFailed,
    Stopped,
    Replaced,
    Cleanup,
}
