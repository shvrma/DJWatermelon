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

        // Register bot's commands.
        string? debugGuildIdStr = _config["TestingGuildId"];
        if (_hostEnvironment.IsDevelopment())
        {
            if (!string.IsNullOrWhiteSpace(debugGuildIdStr) &&
                Snowflake.TryParse(debugGuildIdStr, out Snowflake? debugGuildSnowflake))
            {
                await _slashService.UpdateSlashCommandsAsync(
                    debugGuildSnowflake, ct: stoppingToken);
            }
            else
            {
                _logger.LogTestingGuildIdMisplaced();
            }
        }
        else
        {
            await _slashService.UpdateSlashCommandsAsync(ct: stoppingToken);
        }

        _discordClient.SubmitCommand(
            new UpdatePresence(
                Status: UserStatus.Idle,
                IsAFK: false,
                Since: DateTimeOffset.Now,
                Activities: new List<IActivity>
                {
                    new Activity(
                        Name: "/help",
                        Type: ActivityType.Listening)
                }));

        _logger.LogDiscordWrapperReady();
    }
}
