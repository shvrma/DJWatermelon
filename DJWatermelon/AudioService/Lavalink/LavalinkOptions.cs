using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink;

internal class LavalinkOptions
{
    [NotNull]
    public string? WebSocketUri { get; set; }

    [NotNull]
    public string? Authorization { get; set; }

    [NotNull]
    public string? UserId { get; set; }
}
