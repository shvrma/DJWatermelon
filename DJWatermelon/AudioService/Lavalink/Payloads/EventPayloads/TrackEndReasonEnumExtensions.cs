using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

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
