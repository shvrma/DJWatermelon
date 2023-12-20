using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;
using System.Threading.Channels;

namespace DJWatermelon.AudioService;

internal class VoiceStateUpdateResponder : IResponder<IVoiceStateUpdate>
{
    private readonly VoiceStatesService _voiceStates;
    private readonly ILogger<VoiceStateUpdateResponder> _logger;
    private readonly IUser _bot;

    public VoiceStateUpdateResponder(
        VoiceStatesService voiceStates,
        ILogger<VoiceStateUpdateResponder> logger,
        IDiscordRestUserAPI userAPI)
    {
        _voiceStates = voiceStates;
        _logger = logger;
        _bot = userAPI.GetCurrentUserAsync().Result.Entity;
    }

    async Task<Result> IResponder<IVoiceStateUpdate>.RespondAsync(
        IVoiceStateUpdate voiceStateUpdate,
        CancellationToken cancellationToken)
    {
        // Check if the user that changed the state is us.
        if (_bot.ID != voiceStateUpdate.UserID)
        {
            return Result.Success;
        }

        // Expliptical checks whatever log level is enabled to
        // ensure we don't resolve the guild name insolently.
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogVoiceStateUpdated(
                voiceStateUpdate.UserID.ToString(),
                voiceStateUpdate.ToString() ?? string.Empty);
        }

        // User disconected.
        // Gateway's voice state update payload doesn't include
        // the channel ID when the bot leaves off, so removing
        // the corresponding voice state should be implemented.
        if (voiceStateUpdate.ChannelID == null)
        {
            return Result.Success;
        }
        // A user connected or its state updated.
        else
        {
            Snowflake voiceChannelID = voiceStateUpdate.ChannelID.Value;

            // Update value.
            if (_voiceStates.VoiceSessionsCache.TryGetValue(
                voiceChannelID, out VoiceSession? voiceSession))
            {
                _voiceStates.VoiceSessionsCache[voiceChannelID] = voiceSession with
                {
                    SessionId = voiceStateUpdate.SessionID
                };
            }
            // Add new voice session.
            else
            {
                voiceSession = new VoiceSession(voiceStateUpdate.SessionID);

                _voiceStates.VoiceSessionsCache.Add(voiceChannelID, voiceSession);

                // If there is an open channel, write to it.
                if (_voiceStates.VoiceSessionsChannels.TryGetValue(
                    voiceChannelID, out Channel<VoiceSession>? channel))
                {
                    await channel.Writer.WriteAsync(voiceSession, cancellationToken);
                    channel.Writer.Complete();
                }
            }
        }

        return Result.Success;
    }
}
