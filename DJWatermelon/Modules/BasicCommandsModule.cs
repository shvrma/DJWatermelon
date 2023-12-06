using Discord.Audio;
using Discord.Interactions;

namespace DJWatermelon.Modules;

public class BasicCommandsModule : InteractionModuleBase
{
    [SlashCommand(
        "ping",
        "Try pinging the bot to see if you get a response.")]
    public async Task PingAsync()
    {
        await RespondAsync("Pong!");
    }

    [SlashCommand(
        "join",
        "Joins to the passed voice chat or, if omitted, to the same as you.",
        runMode: RunMode.Async)]
    public async Task JoinVoiceAsync(IVoiceChannel? channel = default)
    {
        channel ??= (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null)
        {
            await RespondAsync("You should be in any voice chat or pass it as a command param " +
                "before executing it. :nerd::point_up:");
            return;
        }

        IAudioClient audioClient = await channel.ConnectAsync();
        await RespondAsync($"Successfully joined {channel.Mention}.");

        audioClient.CreateOpusStream();
    }
}
