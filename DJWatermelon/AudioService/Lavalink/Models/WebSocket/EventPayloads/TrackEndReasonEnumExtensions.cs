namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;

internal static class TrackEndReasonEnumExtensions
{
    public static bool MayStartNext(this TrackEndReason reason)
    {
        return reason switch
        {
            TrackEndReason.Finished => true,
            TrackEndReason.LoadFailed => true,
            TrackEndReason.Stopped => false,
            TrackEndReason.Replaced => false,
            TrackEndReason.Cleanup => false,
            _ => throw new InvalidOperationException(nameof(reason)),
        };
    }
}
