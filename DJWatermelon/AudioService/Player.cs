using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService;

internal abstract class Player
{
    private readonly IAudioClient _audioClient;

    public Player(IAudioClient audioClient)
    {
        _audioClient = audioClient;
    }
}
