global using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DJWatermelon;
using DJWatermelon.AudioService;
using DJWatermelon.AudioService.Lavalink;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using YoutubeExplode;

HostApplicationBuilder hostBuilder =
    Host.CreateApplicationBuilder();

hostBuilder.Services.AddSingleton<DiscordSocketClient>();
hostBuilder.Services.AddSingleton<InteractionService>();
hostBuilder.Services.AddSingleton<YoutubeClient>();

hostBuilder.Services.AddHostedService<DiscordWrapperHostedService>();

// By default, do all the audio encoding/decoding and streaming
// functionality on the application host - otherwise - use Lavalink.
if (hostBuilder.Configuration.GetValue<bool>("UseInternalAudioProcessing"))
{
    // TODO.
}
else
{
    hostBuilder.Services.AddHostedService<LavalinkPlayersManager>();
    hostBuilder.Services.AddSingleton<IPlayersManager, LavalinkPlayersManager>();
    hostBuilder.Services.Configure<LavalinkOptions>(
        hostBuilder.Configuration.GetSection(key: nameof(LavalinkOptions)));
}

hostBuilder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.GuildVoiceStates | GatewayIntents.Guilds
});

hostBuilder.Services.AddSingleton(new InteractionServiceConfig
{
    UseCompiledLambda = true
});

hostBuilder.Build().Run();
