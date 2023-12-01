using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon;

internal class Player
{
    private readonly IAudioClient _audioClient;
    private readonly Lazy<AudioOutStream> _stream;

    public Player(IAudioClient audioClient)
    {
        _audioClient = audioClient;
        _stream = new Lazy<AudioOutStream>(_audioClient.CreateOpusStream());
    }

    public async Task PlayAsync()
    {
        
    }
}
