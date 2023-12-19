using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService;

internal class VoiceServerUpdateResponder : IResponder<IVoiceServerUpdate>
{
    private readonly VoiceStatesService _voiceStates;
    private readonly ILogger<VoiceServerUpdateResponder> _logger;

    public VoiceServerUpdateResponder(
        VoiceStatesService voiceStates,
        ILogger<VoiceServerUpdateResponder> logger)
    {
        _voiceStates = voiceStates;
        _logger = logger;
    }

    async Task<Result> IResponder<IVoiceServerUpdate>.RespondAsync(
        IVoiceServerUpdate serverUpdate, CancellationToken cancellationToken)
    {
        // Expliptical checks whatever log level is enabled to 
        // ensure we don't resolve the guild name insolently. 
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogVoiceServerUpdate(
                serverUpdate.GuildID.ToString(),
                serverUpdate.ToString() ?? string.Empty);
        }

        // Cache guild's voice server.
        // Update existing voice server.
        if (_voiceStates.VoiceServersCache.TryGetValue(
            serverUpdate.GuildID,
            out VoiceServer? voiceServer))
        {
            _voiceStates.VoiceServersCache[serverUpdate.GuildID] = voiceServer with
            {
                Token = serverUpdate.Token,
                Endpoint = serverUpdate.Endpoint
            };
        }
        // A new voice server.
        else
        {
            voiceServer = new VoiceServer(
                Token: serverUpdate.Token,
                Endpoint: serverUpdate.Endpoint);

            _voiceStates.VoiceServersCache.Add(
                serverUpdate.GuildID, voiceServer);

            // If there is an open channel, write to it.
            if (_voiceStates.VoiceServersChannels.TryGetValue(
                serverUpdate.GuildID,
                out Channel<VoiceServer>? channel))
            {
                await channel.Writer.WriteAsync(voiceServer, cancellationToken);
                channel.Writer.Complete();
            }
        }

        return Result.Success;
    }
}
