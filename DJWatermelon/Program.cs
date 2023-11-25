global using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DJWatermelon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

HostApplicationBuilder hostBuilder =
    Host.CreateApplicationBuilder();

hostBuilder.Services.AddSingleton<DiscordSocketClient>();
hostBuilder.Services.AddSingleton<InteractionService>();

hostBuilder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.GuildVoiceStates
});

hostBuilder.Services.AddHostedService<DiscordWrapperHostedService>();

hostBuilder.Build().Run();
