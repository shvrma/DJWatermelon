using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService;

internal interface IPlayersManager : IHostedService, IDisposable, IAsyncDisposable
{
    IPlayer CreatePlayer(long guilId);

    bool TryGetPlayer(ulong id, [NotNullWhen(true)] out IPlayer? player);

    IEnumerable<IPlayer> GetPlayers();

    void DestroyPlayer(ulong guilId);

    Task<IEnumerable<ITrackHandle>> SearchForTrackAsync(string prompt);
}
