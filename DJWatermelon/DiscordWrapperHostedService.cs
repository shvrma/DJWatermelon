using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DJWatermelon;

internal class DiscordWrapperHostedService : BackgroundService
{
    private readonly DiscordSocketClient _discordClient;
    private readonly InteractionService _interactionService;
    private readonly ILogger<DiscordWrapperHostedService> _logger;
    private readonly ILogger<DiscordSocketClient> _discordLogger;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IServiceProvider _serviceProvider;

    public DiscordWrapperHostedService(
        DiscordSocketClient client,
        InteractionService interactionService,
        ILogger<DiscordWrapperHostedService> logger,
        ILogger<DiscordSocketClient> discordLogger,
        IConfiguration config,
        IHostEnvironment hostEnvironment,
        IServiceProvider serviceProvider)
    {
        _discordClient = client;
        _logger = logger;
        _discordLogger = discordLogger;
        _config = config;
        _interactionService = interactionService;
        _hostEnvironment = hostEnvironment;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start preparation as a connection establishment and set up all the event handlers.
        _discordClient.Log += msg =>
        {
            _discordLogger.LogDiscordWrapperMessage(
                logLevel: (LogLevel)(5 - msg.Severity),
                msg.Exception,
                msg.Message);

            return Task.CompletedTask;
        };

        await _discordClient.LoginAsync(TokenType.Bot, _config["DiscordToken"]);
        await _discordClient.StartAsync();

        // Expicticly wait until the wrapper is ready as it initializes on another thread.
        bool isDiscordWrapperReady = false;
        _discordClient.Ready += () =>
        {
            isDiscordWrapperReady = true;
            return Task.CompletedTask;
        };
        SpinWait.SpinUntil(() => isDiscordWrapperReady);

        _logger.LogDiscordWrapperReady();

        // Register bot's commands.
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

        ulong debugGuildId = _config.GetValue<ulong>("DebugGuildId");
        if (_hostEnvironment.IsDevelopment())
        {
            await _interactionService.RegisterCommandsToGuildAsync(debugGuildId);
        }
        else
        {
            await _interactionService.RegisterCommandsGloballyAsync();
        }

        // Set a command's handlers.
        _discordClient.SlashCommandExecuted += SlashCommandHandler;
    }

    private async Task SlashCommandHandler(SocketSlashCommand cmd)
    {
        using IDisposable? loggingScope = _logger.BeginScope(cmd);
        _logger.LogSlashCommandReceived();

        SocketInteractionContext ctx = new(_discordClient, cmd);
        IResult execRslt =
            await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);

        if (!execRslt.IsSuccess)
        {
            _logger.LogSlashCommandFailed();


        }
    }
}
