using Discord.Audio;
using Refit;

namespace DJWatermelon.AudioService.Lavalink;

internal class LavalinkPlayer : IPlayer
{
    private readonly ILavalinkAPI _lavalinkAPI;
    private ulong _guildId;
    private string _lavalinkSessionId;

    public LavalinkPlayer(
        ulong guildId, 
        string lavalinkSessionId,
        ILavalinkAPI lavalinkAPI)
    {
        _guildId = guildId;
        _lavalinkSessionId = lavalinkSessionId;
        _lavalinkAPI = lavalinkAPI;
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
