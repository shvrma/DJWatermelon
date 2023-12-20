using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DJWatermelon.AudioService;

internal class AudioServiceHostedService : IHostedService
{
    private readonly IPlayersManager _playersManager;
    private readonly ILogger<AudioServiceHostedService> _logger;

    public AudioServiceHostedService(
        IPlayersManager playersManager,
        ILogger<AudioServiceHostedService> logger)
    {
        _playersManager = playersManager;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogAudioServiceInitStarted();
        await _playersManager.InitAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogAudioServiceStopRequested();
        await _playersManager.DisposeAsync();
    }
}
