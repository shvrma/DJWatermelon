using DJWatermelon;
using DJWatermelon.AudioService;
using DJWatermelon.AudioService.Lavalink;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Extensions.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
using Remora.Discord.Rest;
using System.Reflection;
using YoutubeExplode;

HostApplicationBuilder hostBuilder =
    Host.CreateApplicationBuilder();

string? token = hostBuilder.Configuration["DiscordToken"];
ArgumentNullException.ThrowIfNull(token, nameof(token));

hostBuilder.Services
    .AddDiscordGateway(_ => token)
    .AddDiscordCaching()
    .AddDiscordCommands(enableSlash: true)
    .AddCommandGroupsFromAssembly(Assembly.GetExecutingAssembly())
    .AddRespondersFromAssembly(Assembly.GetExecutingAssembly())
    .Configure<DiscordGatewayClientOptions>(options => 
    {
        options.Intents = GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates;
    });

hostBuilder.Services.AddSingleton<YoutubeClient>();

hostBuilder.Services
    .AddHostedService<DiscordWrapperHostedService>()
    .AddHostedService<AudioServiceHostedService>();

// By default, do all the audio encoding/decoding and streaming
// functionality on the application host - otherwise - use Lavalink.
if (hostBuilder.Configuration.GetValue<bool>("UseInternalAudioProcessing"))
{
    throw new NotImplementedException("Internal audio processing has not yet been implemented.");
}
else
{
    hostBuilder.Services.AddSingleton<IPlayersManager, LavalinkPlayersManager>();

    hostBuilder.Services
        .AddOptionsWithValidateOnStart<LavalinkOptions>()
        .BindConfiguration(nameof(LavalinkOptions));
}

hostBuilder.Build().Run();
