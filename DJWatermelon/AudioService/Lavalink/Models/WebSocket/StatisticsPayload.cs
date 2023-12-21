using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink.Models.WebSocket;

public sealed record StatisticsPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("players")]
    ushort ConnectedPlayers,

    [property: JsonRequired]
    [property: JsonPropertyName("playingPlayers")]
    ushort PlayingPlayers,

    [property: JsonRequired]
    [property: JsonPropertyName("uptime")]
    ulong Uptime,

    [property: JsonRequired]
    [property: JsonPropertyName("memory")]
    ServerMemoryUsageStatisticsModel MemoryUsage,

    [property: JsonRequired]
    [property: JsonPropertyName("cpu")]
    ServerProcessorUsageStatisticsModel ProcessorUsage,

    [property: JsonRequired]
    [property: JsonPropertyName("frameStats")]
    ServerFrameStatisticsModel? FrameStatistics) : IWebSocketPayload;

public sealed record class ServerMemoryUsageStatisticsModel(
    [property: JsonRequired]
    [property: JsonPropertyName("free")]
    ulong FreeMemory,

    [property: JsonRequired]
    [property: JsonPropertyName("used")]
    ulong UsedMemory,

    [property: JsonRequired]
    [property: JsonPropertyName("allocated")]
    ulong AllocatedMemory,

    [property: JsonRequired]
    [property: JsonPropertyName("reservable")]
    ulong ReservableMemory);

public sealed record class ServerProcessorUsageStatisticsModel(
    [property: JsonRequired]
    [property: JsonPropertyName("cores")]
    ushort CoreCount,

    [property: JsonRequired]
    [property: JsonPropertyName("systemLoad")]
    float SystemLoad,

    [property: JsonRequired]
    [property: JsonPropertyName("lavalinkLoad")]
    float LavalinkLoad);

public sealed record ServerFrameStatisticsModel(
    [property: JsonRequired]
    [property: JsonPropertyName("sent")]
    uint SentFrames,

    [property: JsonRequired]
    [property: JsonPropertyName("nulled")]
    uint NulledFrames,

    [property: JsonRequired]
    [property: JsonPropertyName("deficit")]
    int DeficitFrames);
