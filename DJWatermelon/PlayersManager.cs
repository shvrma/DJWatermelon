using Discord.Audio;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DJWatermelon;

internal class PlayersManager
{
    private readonly ConcurrentDictionary<ulong, Player> _players = new();
    private readonly IHostEnvironment _environment;

    public PlayersManager(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool TryGet(ulong id, [MaybeNullWhen(false)] out Player? player)
        => _players.TryGetValue(id, out player);

    public Player CreatePlayer(ulong guildId, IAudioClient audioClient)
    {
        Player player = new(
            audioClient);

        _players.TryAdd(guildId, player);
        return player;
    }
}
