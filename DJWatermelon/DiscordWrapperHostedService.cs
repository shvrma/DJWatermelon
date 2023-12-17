using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Rest.Core;
using Remora.Results;
using System.Reflection;

namespace DJWatermelon;

internal class DiscordWrapperHostedService : BackgroundService
{
    private readonly DiscordGatewayClient _discordClient;
    private readonly ILogger<DiscordWrapperHostedService> _logger;
    private readonly ILogger<DiscordGatewayClient> _discordLogger;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IServiceProvider _serviceProvider;
    private readonly SlashService _slashService;

    public DiscordWrapperHostedService(
        DiscordGatewayClient client,
        ILogger<DiscordWrapperHostedService> logger,
        ILogger<DiscordGatewayClient> discordLogger,
        IConfiguration config,
        IHostEnvironment hostEnvironment,
        IServiceProvider serviceProvider,
        SlashService slashService)
    {
        _discordClient = client;
        _logger = logger;
        _discordLogger = discordLogger;
        _config = config;
        _hostEnvironment = hostEnvironment;
        _serviceProvider = serviceProvider;
        _slashService = slashService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordLogger.LogConnectionEstablishment();

        // Start preparation as a connection establishment and set up all the event handlers.
        await _discordClient.RunAsync(stoppingToken);

        // Check the bot's token presence and connect if it is.
        string? token = _config["DiscordToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogTokenMisplaced();
            ArgumentException.ThrowIfNullOrEmpty(token);
        }
        _discordClient.SubmitCommand(
            new Identify(
                token, 
                new IdentifyConnectionProperties("DJWatermelon")));

        _discordClient.SubmitCommand(
            new UpdatePresence(
                UserStatus.Online, false, null, new List<IActivity> 
                {
                    new Activity("/help", ActivityType.Listening)
                }));

        _logger.LogDiscordWrapperReady();

        // Register bot's commands.
        string? debugGuildIdStr = _config["TestingGuildId"];
        if (_hostEnvironment.IsDevelopment())
        {
            if (!string.IsNullOrWhiteSpace(debugGuildIdStr) && 
                Snowflake.TryParse(debugGuildIdStr, out Snowflake? debugGuildSnowflake))
            {
                await _slashService.UpdateSlashCommandsAsync(debugGuildSnowflake, ct: stoppingToken);
            }
            else
            {
                _logger.LogTestingGuildIdMisplaced();

                await _slashService.UpdateSlashCommandsAsync();
            }
        }
    }
}
