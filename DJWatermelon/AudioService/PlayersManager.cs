using Discord.Audio;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DJWatermelon.AudioService;

internal class PlayersManager<PlayerT> where PlayerT : Player
{
    private readonly ConcurrentDictionary<ulong, PlayerT> _players = new();
    private readonly IHostEnvironment _environment;
    private readonly YoutubeClient _youtubeClient;

    public PlayersManager(
        IHostEnvironment environment,
        YoutubeClient youtubeClient)
    {
        _environment = environment;
        _youtubeClient = youtubeClient;
    }

    public bool TryGet(ulong id, [MaybeNullWhen(false)] out PlayerT? player)
        => _players.TryGetValue(id, out player);

    public async Task<TrackHandle> SearchForTrackAsync(string prompt)
    {
        StreamManifest source = await _youtubeClient.Videos.Streams.GetManifestAsync(prompt);
        IEnumerable<AudioOnlyStreamInfo> streams =
            source.GetAudioOnlyStreams().Where(streamInfo => streamInfo.AudioCodec == "opus");

        Stream stream = await _youtubeClient.Videos.Streams.GetAsync(streams.First());
        return new TrackHandle("test", stream);
    }
}
