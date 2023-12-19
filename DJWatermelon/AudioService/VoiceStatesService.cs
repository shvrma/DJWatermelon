using Microsoft.Extensions.Logging;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using YoutubeExplode.Channels;
using Channel = System.Threading.Channels.Channel;

namespace DJWatermelon.AudioService;

// A possible null endpoint in time between moving 
// from a failed voice server to the newly allocated one.
public sealed record VoiceServer(string Token, string? Endpoint);
public sealed record VoiceSession(string SessionId);

public sealed class VoiceStatesService
{
    private readonly ILogger<VoiceStatesService> _logger;

    public VoiceStatesService(ILogger<VoiceStatesService> logger)
    {
        _logger = logger;
    }

    // Channels used to announce voice server updates and voice state updates.
    // List of guild's voice servers. Every guild has its voice server.
    public IDictionary<Snowflake, Channel<VoiceServer>> VoiceServersChannels { get; } =
        new ConcurrentDictionary<Snowflake, Channel<VoiceServer>>();

    public IDictionary<Snowflake, VoiceServer> VoiceServersCache { get; } =
        new ConcurrentDictionary<Snowflake, VoiceServer>();

    // Voice chat ID serves as a key for each session ID.
    public IDictionary<Snowflake, Channel<VoiceSession>> VoiceSessionsChannels { get; } =
        new ConcurrentDictionary<Snowflake, Channel<VoiceSession>>();

    public IDictionary<Snowflake, VoiceSession> VoiceSessionsCache { get; } =
        new ConcurrentDictionary<Snowflake, VoiceSession>();

    public async ValueTask<(bool IsSet, VoiceServer? Value)> RetrieveVoiceServerAsync(
        Snowflake guildID, CancellationToken cancellationToken)
    {
        // Try to obtain the voice server; if it isn't,
        // create a channel and listen to it.
        if (VoiceServersCache.TryGetValue(
            guildID, out VoiceServer? voiceServer))
        {
            return (true, voiceServer);
        }

        Channel<VoiceServer> channel = Channel.CreateBounded<VoiceServer>(1);
        VoiceServersChannels.Add(guildID, channel);

        if (await channel.Reader.WaitToReadAsync(cancellationToken))
        {
            voiceServer = await channel.Reader.ReadAsync(cancellationToken);

            // After the value is retrieved: dispose channel.
            VoiceServersChannels.Remove(guildID);

            return (true, voiceServer);
        }
        else
        {
            return (false, default);
        }
    }

    public async ValueTask<(bool IsSet, VoiceSession? Value)> RetrieveVoiceSessionAsync(
        Snowflake voiceChannelID, CancellationToken cancellationToken)
    {
        // Try to obtain the voice state; if it isn't,
        // create a channel and listen to it.
        if (VoiceSessionsCache.TryGetValue(
            voiceChannelID, out VoiceSession? voiceSession))
        {
            return (true, voiceSession);
        }

        Channel<VoiceSession> channel = Channel.CreateBounded<VoiceSession>(1);
        VoiceSessionsChannels.Add(voiceChannelID, channel);

        if (await channel.Reader.WaitToReadAsync(cancellationToken))
        {
            voiceSession = await channel.Reader.ReadAsync(cancellationToken);

            // After the value is retrieved: dispose channel.
            VoiceSessionsChannels.Remove(voiceChannelID);

            return (true, voiceSession);
        }
        else
        {
            return (false, default);
        }
    }
}
