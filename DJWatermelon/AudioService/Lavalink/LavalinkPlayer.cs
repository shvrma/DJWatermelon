using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink;

internal class LavalinkPlayer : Player
{
    public LavalinkPlayer(IAudioClient audioClient) : base(audioClient)
    {

    }
}
