using Discord.Params;
using DiscordChannelNameUpdater.Interfaces;
using IDelayHandler = HttpRequestMessageHandler.Interfaces.IDelayHandler;
using IDiscord = Discord.Interfaces.IDiscord;

namespace DiscordChannelNameUpdater;

/// <inheritdoc/>
/// <param name="discord">The <see cref="IDiscord"/>.</param>
/// <param name="config">The <see cref="IConfig"/>.</param>
/// <param name="delayHandler">The <see cref="IDelayHandler"/>.</param>
public sealed class DiscordChannelNameNameUpdater(IDiscord discord, IConfig config, IDelayHandler delayHandler)
    : IDiscordChannelNameUpdater
{
    private bool? _isUserInChannel;

    /// <inheritdoc/>
    /// <exception cref="TaskCanceledException">The cancellation token was cancelled.</exception>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await StepAsync(cancellationToken);
        }

        throw new TaskCanceledException();
    }

    private async Task StepAsync(CancellationToken cancellationToken)
    {
        var getUserVoiceStateChannelIdAsyncParams = new GetUserVoiceStateChannelIdAsyncParams
        {
            GuildId = config.GuildId,
            UserId = config.UserId
        };

        var isUserInChannel = config.ChannelId == await discord.GetUserVoiceStateChannelIdAsync(
            getUserVoiceStateChannelIdAsyncParams,
            cancellationToken);

        if (isUserInChannel == _isUserInChannel)
        {
            return;
        }

        _isUserInChannel = isUserInChannel;

        var name = isUserInChannel ? config.ChannelNameIfConnected : config.ChannelNameIfDisconnected;

        var modifyChannelNameAsyncParams = new ModifyChannelNameAsyncParams
        {
            ChannelId = config.ChannelId,
            Name = name
        };

        await discord.ModifyChannelNameAsync(modifyChannelNameAsyncParams, cancellationToken);

        const int millisecondsDelay = 300_000;

        await delayHandler.Delay(millisecondsDelay, cancellationToken);
    }
}