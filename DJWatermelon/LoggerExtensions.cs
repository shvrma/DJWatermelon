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
        EventName = "DiscordWrapperReady",
        Level = LogLevel.Information,
        Message = "Connection with Discord was established successfully, and all data is synced and up-to-date.")]
    public static partial void LogDiscordWrapperReady(
        this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        EventName = "SlashCommandReceived",
        Level = LogLevel.Debug,
        Message = "The slash command was executed and being handled.")]
    public static partial void LogSlashCommandReceived(
        this ILogger logger);

    [LoggerMessage(
        EventId = 3,
        EventName = "SlashCommandFailed",
        Level = LogLevel.Warning,
        Message = "")]
    public static partial void LogSlashCommandFailed(this ILogger logger);

    #endregion
}
