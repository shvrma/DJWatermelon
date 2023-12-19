using Remora.Rest.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DJWatermelon;

// A possible null endpoint in time between moving 
// from a failed voice server to the newly allocated one.
public sealed record VoiceServer(string Token, string? Endpoint);
public sealed record VoiceSession(string SessionId);

public sealed class VoiceStates
{
    // Channel used to announce voice server updates and voice state updates.
    public IDictionary<Snowflake, Channel<VoiceServer>> VoiceServersChannels { get; } =
        new ConcurrentDictionary<Snowflake, Channel<VoiceServer>>();

    public IDictionary<Snowflake, Channel<VoiceSession>> VoiceSessionsChannels { get; } =
        new ConcurrentDictionary<Snowflake, Channel<VoiceSession>>();

    // List of guild's voice servers. Every guild has its voice server.
    public IDictionary<Snowflake, VoiceServer> VoiceServersCache { get; } =
        new ConcurrentDictionary<Snowflake, VoiceServer>();

    // Voice chat ID serves as a key for each session ID.
    public IDictionary<Snowflake, VoiceSession> VoiceSessionsCache { get; } =
        new ConcurrentDictionary<Snowflake, VoiceSession>();
}
