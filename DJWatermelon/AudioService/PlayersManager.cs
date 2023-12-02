﻿using Discord.Audio;
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

internal class PlayersManager
{
    private readonly ConcurrentDictionary<ulong, IPlayer> _players = new();
    private readonly IHostEnvironment _environment;

    public PlayersManager(
        IHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool TryGet(ulong id, [NotNullWhen(true)] out IPlayer? player)
        => _players.TryGetValue(id, out player);
}