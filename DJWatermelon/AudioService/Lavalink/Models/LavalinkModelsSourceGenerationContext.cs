using DJWatermelon.AudioService.Lavalink.Models.REST;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;
using System.Text.Json.Serialization;

namespace DJWatermelon;

#region WebSocket

[JsonSerializable(typeof(IWebSocketPayload))]
[JsonSerializable(typeof(ReadyPayload))]
[JsonSerializable(typeof(PlayerUpdatePayload))]
[JsonSerializable(typeof(StatisticsPayload))]

[JsonSerializable(typeof(EventPayload))]
[JsonSerializable(typeof(TrackEndEventPayload))]
[JsonSerializable(typeof(TrackExceptionEventPayload))]
[JsonSerializable(typeof(TrackStartEventPayload))]
[JsonSerializable(typeof(TrackStuckEventPayload))]
[JsonSerializable(typeof(WebSocketClosedEventPayload))]

#endregion

#region REST

[JsonSerializable(typeof(IRESTModel))]
[JsonSerializable(typeof(ErrorModel))]
[JsonSerializable(typeof(PlayerModel))]
[JsonSerializable(typeof(PlayerUpdateModel))]
[JsonSerializable(typeof(PlayerTrackUpateModel))]
[JsonSerializable(typeof(VoiceStateModel))]
[JsonSerializable(typeof(TrackLoadResultModel))]
[JsonSerializable(typeof(PlaylistResultDataModel))]
[JsonSerializable(typeof(PlaylistInfoModel))]
[JsonSerializable(typeof(LavalinkInfoModel))]

#endregion

internal partial class LavalinkModelsSourceGenerationContext : JsonSerializerContext
{

}
