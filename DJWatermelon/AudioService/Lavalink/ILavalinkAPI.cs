using DJWatermelon.AudioService.Lavalink.Models;
using DJWatermelon.AudioService.Lavalink.Models.REST;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink;

internal interface ILavalinkAPI
{
    [Get("/loadtracks")]
    Task<TrackLoadResultModel> LoadTracksAsync(string ID);

    [Get("/decodetrack")]
    Task<LavalinkTrackHandle> DecodeTrackAsync(
        [AliasAs("encodedTrack")] string encodedData);

    [Get("/decodetrack")]
    Task<IEnumerable<LavalinkTrackHandle>> DecodeTracksAsync(
        [Body] IEnumerable<string> encodedData);

    [Get("/sessions/{sessionId}/players")]
    Task<IEnumerable<LavalinkPlayer>> GetPlayers(string sessionID);

    [Get("/sessions/{sessionId}/players/{guildId}")]
    Task<LavalinkPlayer> GetPlayerAsync(string sessionId, ulong guildID);

    [Patch("/sessions/{sessionId}/players/{guildId}")]
    Task<PlayerModel> UpateOrCreatePlayerAsync(
        string sessionID, ulong guildID, [Body] PlayerUpdateModel playerUpdate);

    [Delete("/sessions/{sessionId}/players/{guildId}")]
    Task DestroyPlayerAsync(string sessionID, ulong guildID);

    [Get("/info")]
    Task<LavalinkInfoModel> GetLavalinkInfoAsync();
}
