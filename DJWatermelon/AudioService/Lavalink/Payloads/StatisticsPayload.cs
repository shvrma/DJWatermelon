using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

public sealed record class StatisticsPayload(
    [property: JsonRequired]
    [property: JsonPropertyName("players")]
    int ConnectedPlayers,

    [property: JsonRequired]
    [property: JsonPropertyName("playingPlayers")]
    int PlayingPlayers,

    [property: JsonRequired]
    [property: JsonPropertyName("uptime")]
    // TimeSpan.
    long Uptime,

    [property: JsonRequired]
    [property: JsonPropertyName("memory")]
    ServerMemoryUsageStatisticsModel MemoryUsage,

    [property: JsonRequired]
    [property: JsonPropertyName("cpu")]
    ServerProcessorUsageStatisticsModel ProcessorUsage,

    [property: JsonPropertyName("frameStats")]
    ServerFrameStatisticsModel? FrameStatistics) : IPayload
{
    public OperationTypes OperationType { get; init; }
}

public sealed record class ServerMemoryUsageStatisticsModel(
    [property: JsonRequired]
    [property: JsonPropertyName("free")]
    long FreeMemory,

    [property: JsonRequired]
    [property: JsonPropertyName("used")]
    long UsedMemory,

    [property: JsonRequired]
    [property: JsonPropertyName("allocated")]
    long AllocatedMemory,

    [property: JsonRequired]
    [property: JsonPropertyName("reservable")]
    long ReservableMemory);

public sealed record class ServerProcessorUsageStatisticsModel(
    [property: JsonRequired]
    [property: JsonPropertyName("cores")]
    int CoreCount,

    [property: JsonRequired]
    [property: JsonPropertyName("systemLoad")]
    float SystemLoad,

    [property: JsonRequired]
    [property: JsonPropertyName("lavalinkLoad")]
    float LavalinkLoad);

public sealed record ServerFrameStatisticsModel(
    [property: JsonRequired]
    [property: JsonPropertyName("sent")]
    int SentFrames,

    [property: JsonRequired]
    [property: JsonPropertyName("nulled")]
    int NulledFrames,

    [property: JsonRequired]
    [property: JsonPropertyName("deficit")]
    int DeficitFrames);
