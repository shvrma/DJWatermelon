using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Payloads.EventPayloads;

internal interface IEventPayload
{
    ulong GuildId { get; init; }
}
