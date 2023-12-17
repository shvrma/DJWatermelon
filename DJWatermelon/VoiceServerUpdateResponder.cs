using Remora.Discord.API.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon;

internal class VoiceServerUpdateResponder : IResponder<VoiceServerUpdate>
{
    public Task<Result> RespondAsync(
        VoiceServerUpdate voiceServerUpdate, 
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
