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
    private readonly Lazy<AudioOutStream> _stream;
    private readonly string _audioBuffer = Path.GetTempFileName();

    public Player(IAudioClient audioClient)
    {
        _audioClient = audioClient;
        // _stream = new Lazy<AudioOutStream>(_audioClient.CreateOpusStream());
    }

    public async Task PlayAsync(TrackHandle track)
    {

    }

    #region Audio streaming internals

    private Process? CreateStream(string path)
        => Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });

    #endregion
}
