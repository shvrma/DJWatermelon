using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace DJWatermelon.AudioService;

public interface IPlayersManager : IDisposable, IAsyncDisposable
{
    Task InitAsync(CancellationToken cancellationToken);
    bool IsReady { get; }

    Task<IPlayer> CreatePlayerAsync(ulong guildId, CancellationToken cancellationToken);
    Task DestroyPlayerAsync(ulong guildId, CancellationToken cancellationToken);

    bool TryGetPlayer(ulong guilid, [NotNullWhen(true)] out IPlayer? player);
    IEnumerable<IPlayer> GetPlayers();

    Task<IEnumerable<ITrackHandle>> SearchForTrackAsync(string prompt, CancellationToken cancellationToken);
}
