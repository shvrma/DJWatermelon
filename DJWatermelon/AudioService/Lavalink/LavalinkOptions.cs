using System.Diagnostics.CodeAnalysis;

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
