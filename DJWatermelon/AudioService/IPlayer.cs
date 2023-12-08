namespace DJWatermelon.AudioService;

internal interface IPlayer : IDisposable, IAsyncDisposable
{
    LinkedList<ITrackHandle> Queue { get; }

    LinkedListNode<ITrackHandle>? CurrentTrack { get; }

    Task PlayAsync(ITrackHandle track);

    Task PlayQueued();

    Task PlayNextAsync();

    Task PlayPreviousAsync();
}
