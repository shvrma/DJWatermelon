namespace DJWatermelon.AudioService.Lavalink;

internal class LavalinkPlayer : IPlayer
{
    public LavalinkPlayer()
    {
        
    }

    public LinkedList<ITrackHandle> Queue { get; } = new();

    public LinkedListNode<ITrackHandle>? CurrentTrack { get; } = default;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

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
}
