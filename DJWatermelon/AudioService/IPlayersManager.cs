using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace DJWatermelon.AudioService;

internal interface IPlayersManager : IDisposable, IAsyncDisposable
{
    void Init();

    IPlayer CreatePlayer(long guilId);

    bool TryGetPlayer(ulong id, [NotNullWhen(true)] out IPlayer? player);

    IEnumerable<IPlayer> GetPlayers();

    void DestroyPlayer(ulong guilId);

    Task<IEnumerable<ITrackHandle>> SearchForTrackAsync(string prompt);
}
