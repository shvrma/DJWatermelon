using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService;

internal interface IPlayersManager : IHostedService
{
    bool TryGet(ulong id, [NotNullWhen(true)] out IPlayer? player);
}
