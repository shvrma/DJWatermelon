using DJWatermelon.AudioService;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Gateway;
using Remora.Rest.Core;
using Remora.Results;
using System.ComponentModel;

namespace DJWatermelon.Modules;

public class BasicCommandsModule : CommandGroup
{
    private readonly IPlayersManager _playersManager;
    private readonly FeedbackService _feedbackService;
    private readonly ICommandContext _commandContext;
    private readonly DiscordGatewayClient _discordGateway;

    public BasicCommandsModule(
        IPlayersManager playersManager,
        FeedbackService feedbackService,
        ICommandContext commandContext,
        DiscordGatewayClient discordGateway)
    {
        _playersManager = playersManager;
        _feedbackService = feedbackService;
        _commandContext = commandContext;
        _discordGateway = discordGateway;
    }

    [Command("ping")]
    [Description("Try pinging the bot to see if you get a response.")]
    public async Task<Result> PingAsync()
    {
        await _feedbackService.SendContextualAsync("Pong!");
        return Result.Success;
    }

    [Command("join")]
    [Description("Joins to the passed voice chat or, if omitted, to the same as you.")]
    public async Task<Result> JoinVoiceAsync(
        [Option('c', "channel")]
        [Description("Voice channel to connect to.")]
        [ChannelTypes(ChannelType.GuildVoice)]
        IChannel? channel = default)
    {
        // TODO, auto connect to voice, user currently is in.
        Snowflake? voiceChatID = channel?.ID;
        if (voiceChatID == null)
        {
            await _feedbackService.SendContextualAsync(
                "You should be in any voice chat or pass it as a command param " +
                "before executing it. :nerd::point_up:");

            return Result.FromError(new ExceptionError(
                new Exception("Can not retrieve voice channel to connect to.")));
        }

        if (!_commandContext.TryGetGuildID(out Snowflake guildID))
        {
            return Result.FromError(new ExceptionError(
                new Exception("Something happened while retrieving the guild ID.")));
        }

        // Enqueue request and wait.
        _discordGateway.SubmitCommand(
            new UpdateVoiceState(
                GuildID: guildID,
                IsSelfMuted: false,
                IsSelfDeafened: true,
                ChannelID: voiceChatID));

        await Task.Delay(_discordGateway.Latency, CancellationToken);

        try
        {
            await _playersManager.CreatePlayerAsync(
                guildID, voiceChatID.Value, CancellationToken);
        }
        catch (Exception ex)
        {
            return Result.FromError(new ExceptionError(ex));
        }

        await _feedbackService.SendContextualAsync(
            $"Successfully joined <#{voiceChatID}>.");

        return Result.Success;
    }
}
