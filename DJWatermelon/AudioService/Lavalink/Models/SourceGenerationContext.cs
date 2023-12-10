using DJWatermelon.AudioService.Lavalink.Models.REST;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;
using System.Text.Json.Serialization;

namespace DJWatermelon;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(IPayload))]
[JsonSerializable(typeof(ReadyPayload))]
[JsonSerializable(typeof(PlayerUpdatePayload))]
[JsonSerializable(typeof(StatisticsPayload))]

[JsonSerializable(typeof(EventPayload))]
[JsonSerializable(typeof(TrackEndEventPayload))]
[JsonSerializable(typeof(TrackExceptionEventPayload))]
[JsonSerializable(typeof(TrackStartEventPayload))]
[JsonSerializable(typeof(TrackStuckEventPayload))]
[JsonSerializable(typeof(WebSocketClosedEventPayload))]

[JsonSerializable(typeof(IRESTResponceModel))]
[JsonSerializable(typeof(ErrorModel))]
[JsonSerializable(typeof(PlayerModel))]
[JsonSerializable(typeof(PlayerUpdateModel))]
[JsonSerializable(typeof(PlayerTrackUpateModel))]
[JsonSerializable(typeof(VoiceStateModel))]

[JsonSerializable(typeof(TrackLoadResultModel))]
[JsonSerializable(typeof(PlaylistModel))]
[JsonSerializable(typeof(PlaylistInfoModel))]
[JsonSerializable(typeof(LavalinkInfoModel))]
internal partial class PayloadsSourceGenerationContext : JsonSerializerContext
{
}
