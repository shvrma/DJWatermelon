﻿using Microsoft.Extensions.Logging;

namespace DJWatermelon;

internal static partial class LoggerExtensions
{
    #region Discord wrapper messages

    [LoggerMessage(
        EventId = 0,
        EventName = "ConnectionEstablishment",
        Level = LogLevel.Error,
        Message = "Begin connection establishment.")]
    public static partial void LogConnectionEstablishment(this ILogger logger);

    [LoggerMessage(
        EventId = 1,
        EventName = "TokenMisplaced",
        Level = LogLevel.Error,
        Message = "The Discord bot's token was misplaced. Include it to any appropriate config source with the key name <DiscordToken>.")]
    public static partial void LogTokenMisplaced(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        EventName = "DiscordWrapperReady",
        Level = LogLevel.Information,
        Message = "Connection with Discord was established successfully, and all data is synced and up-to-date.")]
    public static partial void LogDiscordWrapperReady(
        this ILogger logger);

    [LoggerMessage(
        EventId = 3,
        EventName = "TestingGuildIdMisplaced",
        Level = LogLevel.Warning,
        Message = "The ID of the guild used for testing is misplaced - consider adding it to any appropriate config source with the key <DebugGuildId>.")]
    public static partial void LogTestingGuildIdMisplaced(this ILogger logger);

    [LoggerMessage(
        EventId = 4,
        EventName = "SlashCommandReceived",
        Level = LogLevel.Debug,
        Message = "The slash command was executed and being handled.")]
    public static partial void LogSlashCommandReceived(
        this ILogger logger);

    [LoggerMessage(
        EventId = 5,
        EventName = "SlashCommandFailed",
        Level = LogLevel.Warning,
        Message = "An error occurred while processing the slash command.")]
    public static partial void LogSlashCommandFailed(this ILogger logger);

    #endregion

    #region Lavalink wrapper messages

    [LoggerMessage(
        EventId = 6,
        EventName = "PayloadForInexistentPlayer",
        Level = LogLevel.Warning,
        Message = "Payload received for an inexistent player with guild id: {guildID}.")]
    public static partial void LogPayloadForInexistentPlayer(this ILogger logger, string guildID);

    [LoggerMessage(
        EventId = 7,
        EventName = "ReceivedPayload",
        Level = LogLevel.Trace,
        Message = "A new payload from Lavalink was received.")]
    public static partial void LogReceivedPayload(this ILogger logger);

    [LoggerMessage(
        EventId = 8,
        EventName = "",
        Level = LogLevel.Debug,
        Message = "Payload's received text: \n\n{text}",
        SkipEnabledCheck = true)]
    public static partial void LogPayloadText(this ILogger logger, string text);

    [LoggerMessage(
        EventId = 9,
        EventName = "MultipleReadyPayloadsReceived",
        Level = LogLevel.Warning,
        Message = "Multiple \"Ready\" payloads were received from the Lavalink server. ")]
    public static partial void LogMultipleReadyPayloadsReceived(this ILogger logger);

    [LoggerMessage(
        EventId = 10,
        EventName = "LavalinkReady",
        Level = LogLevel.Information,
        Message = "The Lavalink wrapper is ready.")]
    public static partial void LogLavalinkReady(this ILogger logger);

    [LoggerMessage(
        EventId = 11,
        EventName = "PayloadReceivedBeforeReady",
        Level = LogLevel.Warning,
        Message = "Payload received before Ready payload received.")]
    public static partial void LogPayloadReceivedBeforeReady(this ILogger logger);

    [LoggerMessage(
        EventId = 12,
        EventName = "WebSocketConnecttionEstablished",
        Level = LogLevel.Information,
        Message = "WebSocket connection established.")]
    public static partial void LogWebSocketConnectionEstablished(this ILogger logger);

    [LoggerMessage(
        EventId = 13,
        EventName = "BadPayloadReceived",
        Level = LogLevel.Warning,
        Message = "Bad payload received.")]
    public static partial void LogBadPayloadReceived(this ILogger logger);

    [LoggerMessage(
        EventId = 14,
        EventName = "BufferOutOfRange",
        Level = LogLevel.Error,
        Message = "Message buffer out of free space.")]
    public static partial void LogBufferOutOfRange(this ILogger logger);

    [LoggerMessage(
        EventId = 15,
        EventName = "RemoteHostClosedConnection",
        Level = LogLevel.Error,
        Message = "Websocket Close Connection message received.")]
    public static partial void LogRemoteHostClosedConnection(this ILogger logger);

    [LoggerMessage(
        EventId = 16,
        EventName = "VoiceServerUpdate",
        Level = LogLevel.Debug,
        Message = "Discord voice server updated for guild <{guildID}>.\n\n{voiceServer}",
        SkipEnabledCheck = true)]
    public static partial void LogVoiceServerUpdate(
        this ILogger logger,
        string guildID,
        string voiceServer);

    [LoggerMessage(
        EventId = 17,
        EventName = "VoiceStateUpdated",
        Level = LogLevel.Debug,
        Message = "Voice state updated for user <{userID}>.\n\n<{voiceState}>",
        SkipEnabledCheck = true)]
    public static partial void LogVoiceStateUpdated(
        this ILogger logger,
        string userID,
        string voiceState);

    [LoggerMessage(
        EventId = 18,
        EventName = "CreatingPlayer",
        Level = LogLevel.Debug,
        Message = "Begin creating a player for a guild with id: <{guildId}>.")]
    public static partial void LogCreatingPlayer(this ILogger logger, ulong guildId);

    #region Events

    [LoggerMessage(
        EventName = "TrackStartEvent",
        Level = LogLevel.Debug,
        Message = "{payload}")]
    public static partial void LogTrackStartEvent(this ILogger logger, string payload);

    [LoggerMessage(
        EventName = "TrackEndEvent",
        Level = LogLevel.Debug,
        Message = "{payload}")]
    public static partial void LogTrackEndEvent(this ILogger logger, string payload);

    [LoggerMessage(
        EventName = "TrackExceptionEvent",
        Level = LogLevel.Debug,
        Message = "{payload}")]
    public static partial void LogTrackExceptionEvent(this ILogger logger, string payload);

    [LoggerMessage(
        EventName = "TrackStuckEvent",
        Level = LogLevel.Debug,
        Message = "{payload}")]
    public static partial void LogTrackStuckEvent(this ILogger logger, string payload);

    #endregion

    [LoggerMessage(
        EventId = 19,
        EventName = "LavalinkDisposed",
        Level = LogLevel.Warning,
        Message = "The Lavalink wrapper was disposed of.")]
    public static partial void LogLavalinkDisposed(this ILogger logger);

    #endregion

    #region Other

    [LoggerMessage(
        EventName = "LogAudioServiceInit",
        Level = LogLevel.Debug)]
    public static partial void LogAudioServiceInitStarted(this ILogger logger);

    [LoggerMessage(
        EventName = "LogAudioServiceStop",
        Level = LogLevel.Debug)]
    public static partial void LogAudioServiceStopRequested(this ILogger logger);

    #endregion
}
