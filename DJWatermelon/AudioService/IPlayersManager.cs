using Microsoft.Extensions.Hosting;
using Remora.Rest.Core;
using System.Diagnostics.CodeAnalysis;

namespace DJWatermelon.AudioService;

public interface IPlayersManager : IDisposable, IAsyncDisposable
{
    Task InitAsync(CancellationToken cancellationToken);

    Task<IPlayer> CreatePlayerAsync(Snowflake guilId, CancellationToken cancellationToken);

    Task DestroyPlayerAsync(Snowflake guilId, CancellationToken cancellationToken);

    bool TryGetPlayer(Snowflake id, [NotNullWhen(true)] out IPlayer? player);

    IEnumerable<IPlayer> GetPlayers();

    Task<IEnumerable<ITrackHandle>> SearchForTrackAsync(
        string prompt, 
        CancellationToken cancellationToken);
}
