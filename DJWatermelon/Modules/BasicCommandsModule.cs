using DJWatermelon.AudioService;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Gateway;
using Remora.Rest.Core;
using Remora.Results;
using System.ComponentModel;
using System.Diagnostics;

namespace DJWatermelon.Modules;

public class BasicCommandsModule : CommandGroup
{
    private readonly IPlayersManager _playersManager;
    private readonly FeedbackService _feedbackService;
    private readonly ICommandContext _commandContext;
    private readonly DiscordGatewayClient _discordGateway;
    private readonly IDiscordRestUserAPI _userAPI;
    private readonly IDiscordRestGuildAPI _guildAPI;
    private readonly IDiscordRestChannelAPI _channelAPI;

    public BasicCommandsModule(
        IPlayersManager playersManager,
        FeedbackService feedbackService,
        ICommandContext commandContext,
        DiscordGatewayClient discordGateway,
        IDiscordRestUserAPI userAPI)
    {
        _playersManager = playersManager;
        _feedbackService = feedbackService;
        _commandContext = commandContext;
        _discordGateway = discordGateway;
        _userAPI = userAPI;
    }

    [Command("ping")]
    [Description("Try pinging the bot to see if you get a response.")]
    public async Task PingAsync()
    {
        await _feedbackService.SendContextualAsync("Pong!");
    }
    
    [Command("join")]
    [Description("Joins to the passed voice chat or, if omitted, to the same as you.")]
    public async Task JoinVoiceAsync(
        [Option('c', "channel")]
        [Description("Voice channel to connect to.")]
        [ChannelTypes(ChannelType.GuildVoice)]
        IChannel? channel = default,
        
        IPartialVoiceState? voiceState = default)
    {
        Snowflake? voiceChatID = channel?.ID ?? voiceState?.ChannelID.Value;
        if (!voiceChatID.HasValue)
        {
            await _feedbackService.SendContextualAsync(
                "You should be in any voice chat or pass it as a command param " +
                "before executing it. :nerd::point_up:");
            return;
        }

        if (!_commandContext.TryGetGuildID(out Snowflake guildID))
        {
            throw new Exception("Something happened while retrieving the guild ID.");
        }

        _discordGateway.SubmitCommand(
            new UpdateVoiceState(
                GuildID: guildID, 
                IsSelfMuted: false,
                IsSelfDeafened: true, 
                ChannelID: voiceChatID));
        try
        {
            // await _playersManager.CreatePlayerAsync(guildID, CancellationToken);
        }
        catch (Exception)
        {
            Debugger.Break();
        }

        await _feedbackService.SendContextualAsync(
            $"Successfully joined <@{voiceChatID}>.");
    }
}
