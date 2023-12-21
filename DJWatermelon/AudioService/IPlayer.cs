using Remora.Discord.API.Abstractions.Objects;

namespace DJWatermelon.AudioService;

public interface IPlayer : IDisposable, IAsyncDisposable
{
    Func<IReadOnlyList<IEmbed>, ValueTask>? SendMessageAsync { get; set; }

    LinkedList<ITrackHandle> Queue { get; }

    LinkedListNode<ITrackHandle>? CurrentTrackInqueue { get; set; }

    LoopModes LoopMode { get; set; }

    ITrackHandle? CurrentTrack { get; set; }

    Task PlayAsync(ITrackHandle track);

    Task PlayQueued();

    Task PlayNextAsync();

    Task PlayPreviousAsync();
}

public enum LoopModes
{
    None,
    Queue,
    Track
}
