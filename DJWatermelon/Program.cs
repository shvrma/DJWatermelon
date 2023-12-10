global using Discord;

using Discord.Interactions;
using Discord.WebSocket;
using DJWatermelon;
using DJWatermelon.AudioService;
using DJWatermelon.AudioService.Lavalink;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using YoutubeExplode;

HostApplicationBuilder hostBuilder =
    Host.CreateApplicationBuilder();

hostBuilder.Services
    .AddSingleton<DiscordSocketClient>()
    .AddSingleton<YoutubeClient>()
    .AddSingleton<InteractionService>();

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

hostBuilder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.GuildVoiceStates | GatewayIntents.Guilds
});

hostBuilder.Services.AddSingleton(new InteractionServiceConfig
{
    UseCompiledLambda = true
});

hostBuilder.Build().Run();
