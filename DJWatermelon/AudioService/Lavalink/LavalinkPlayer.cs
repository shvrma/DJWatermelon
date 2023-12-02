using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink;

internal class LavalinkPlayer : IPlayer
{
    public LavalinkPlayer(IAudioClient audioClient)
    {

    }

    public LinkedList<ITrackHandle> Queue => throw new NotImplementedException();

    public LinkedListNode<ITrackHandle>? CurrentTrack => throw new NotImplementedException();

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
