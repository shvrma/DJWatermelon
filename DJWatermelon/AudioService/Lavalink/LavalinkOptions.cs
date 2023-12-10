using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DJWatermelon.AudioService.Lavalink;

internal class LavalinkOptions
{
    [NotNull]
    [DataType(DataType.Url)]
    public string? RemoteHostName { get; set; }

    [NotNull]
    public int? Port { get; set; }

    [NotNull]
    public string? Authorization { get; set; }
}
