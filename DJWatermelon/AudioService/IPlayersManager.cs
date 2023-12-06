using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace DJWatermelon.AudioService;

internal interface IPlayersManager : IHostedService
{
    bool TryGet(ulong id, [NotNullWhen(true)] out IPlayer? player);
}
