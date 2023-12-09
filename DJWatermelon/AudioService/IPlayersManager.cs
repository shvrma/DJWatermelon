using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace DJWatermelon.AudioService;

internal interface IPlayersManager : IDisposable, IAsyncDisposable
{
    Task InitAsync();

    Task<IPlayer> CreatePlayerAsync(long guilId);

    Task DestroyPlayerAsync(ulong guilId);

    bool TryGetPlayer(ulong id, [NotNullWhen(true)] out IPlayer? player);

    IEnumerable<IPlayer> GetPlayers();

    Task<IEnumerable<ITrackHandle>> SearchForTrackAsync(string prompt);
}
