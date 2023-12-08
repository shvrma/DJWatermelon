using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink;

internal class LavalinkHostedService : IHostedService
{
    private readonly LavalinkPlayersManager _playersManager;

    public LavalinkHostedService(LavalinkPlayersManager playersManager)
    {
        _playersManager = playersManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _playersManager.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await (_playersManager as IAsyncDisposable).DisposeAsync();
    }
}
