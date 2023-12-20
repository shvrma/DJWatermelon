using Remora.Rest.Core;
using System.Diagnostics.CodeAnalysis;

namespace DJWatermelon.AudioService;

public interface IPlayersManager : IDisposable, IAsyncDisposable
{
    Task InitAsync(CancellationToken cancellationToken);

    Task<IPlayer> CreatePlayerAsync(Snowflake guildID, Snowflake voiceChatID, CancellationToken cancellationToken);

    Task DestroyPlayerAsync(Snowflake guildID, CancellationToken cancellationToken);

    // Deals with cached players.
    bool TryGetPlayer(Snowflake guidlID, [NotNullWhen(true)] out IPlayer? player);
    IEnumerable<IPlayer> GetPlayers();

    Task<IEnumerable<ITrackHandle>> SearchForTrackAsync(
        string prompt, 
        CancellationToken cancellationToken);
}
