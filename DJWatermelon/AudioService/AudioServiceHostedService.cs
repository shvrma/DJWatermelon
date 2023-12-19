using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService;

internal class AudioServiceHostedService : IHostedService
{
    private readonly IPlayersManager _playersManager;

    public AudioServiceHostedService(IPlayersManager playersManager)
    {
        _playersManager = playersManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            await _playersManager.InitAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            await _playersManager.DisposeAsync();
        }
    }
}
