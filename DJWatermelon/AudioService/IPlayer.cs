using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
