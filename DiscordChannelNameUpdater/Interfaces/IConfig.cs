namespace DiscordChannelNameUpdater.Interfaces;

/// <summary>
/// A <see cref="DiscordChannelNameNameUpdater"/> config.
/// </summary>
public interface IConfig
{
    /// <summary>
    /// Get the ID of the guild.
    /// </summary>
    /// <returns>The ID of the guild.</returns>
    string GuildId { get; }

    /// <summary>
    /// Get the ID of the user.
    /// </summary>
    /// <returns>The ID of the user.</returns>
    string UserId { get; }

    /// <summary>
    /// Get the ID of the channel.
    /// </summary>
    /// <returns>The ID of the channel.</returns>
    string ChannelId { get; }

    /// <summary>
    /// Get the channel names to update to when the user is not in the channel.
    /// </summary>
    /// <returns>The channel names to update to when the user is not in the channel.</returns>
    string ChannelNameIfFalse { get; }

    /// <summary>
    /// Get the channel names to update to when the user is in the channel.
    /// </summary>
    /// <returns>The channel names to update to when the user is in the channel.</returns>
    string ChannelNameIfTrue { get; }
}