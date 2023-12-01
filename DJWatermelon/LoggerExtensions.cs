using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 0,
        EventName = "DiscordWrapperMessage",
        Message = "{message}")]
    public static partial void LogDiscordWrapperMessage(
        this ILogger<DiscordSocketClient> logger,
        LogLevel logLevel,
        Exception exception,
        string message);

    #region Discord wrapper messages

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
        EventName = "EventPayloadForInexistentPlayer",
        Level = LogLevel.Warning,
        Message = "Event payload received for an inexistent player with guild id: {guildId}.")]
    public static partial void LogEventPayloadForInexistentPlayer(this ILogger logger, ulong guildId);

    [LoggerMessage(
        EventId = 7,
        EventName = "ReceivedPayload",
        Level = LogLevel.Trace,
        Message = "A new payload from Lavalink was received.\n\n{serializedPayload}")]
    public static partial void LogReceivedPayload(this ILogger logger, string serializedPayload);

    #endregion
}
