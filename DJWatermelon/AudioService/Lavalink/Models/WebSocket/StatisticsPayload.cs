using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

    [property: JsonPropertyName("frameStats")]
    ServerFrameStatisticsModel? FrameStatistics) : IPayload;

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
    int SentFrames,

    [property: JsonRequired]
    [property: JsonPropertyName("nulled")]
    int NulledFrames,

    [property: JsonRequired]
    [property: JsonPropertyName("deficit")]
    int DeficitFrames);
