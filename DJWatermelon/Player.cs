using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;

namespace DJWatermelon;

internal class Player
{
    private readonly IAudioClient _audioClient;
    private readonly Lazy<AudioOutStream> _stream;
    private readonly Command _ffmpegRunner;

    public Player(IAudioClient audioClient, Command ffmpegRunner)
    {
        _audioClient = audioClient;
        _stream = new Lazy<AudioOutStream>(_audioClient.CreateOpusStream());
        _ffmpegRunner = ffmpegRunner;
    }

    public async Task PlayAsync()
    {
        
    }
}
