using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;

namespace DJWatermelon.AudioService.Lavalink;

internal class LavalinkPlayer : IPlayer
{
    private readonly Snowflake _guildID;
    private readonly LavalinkPlayersManager _playerManager;

    private bool _disposed;

    public LavalinkPlayer(
        Snowflake guildID,
        LavalinkPlayersManager playerManager)
    {
        _guildID = guildID;
        _playerManager = playerManager;
    }

    public Func<IReadOnlyList<IEmbed>, ValueTask>? SendMessageAsync { get; set; }

    public LinkedList<ITrackHandle> Queue { get; } = new();

    public LinkedListNode<ITrackHandle>? CurrentTrackInqueue { get; set; }

    public LoopModes LoopMode { get; set; } = LoopModes.None;

    public ITrackHandle? CurrentTrack { get; set; }

    public Task PlayAsync(ITrackHandle track)
    {
        throw new NotImplementedException();
    }

    public Task PlayNextAsync()
    {
        throw new NotImplementedException();
    }

    public Task PlayPreviousAsync()
    {
        throw new NotImplementedException();
    }

    public Task PlayQueued()
    {
        throw new NotImplementedException();
    }

    void IDisposable.Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _playerManager.DestroyPlayerAsync(_guildID, CancellationToken.None).Wait();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await _playerManager.DestroyPlayerAsync(_guildID, CancellationToken.None);
    }
}
