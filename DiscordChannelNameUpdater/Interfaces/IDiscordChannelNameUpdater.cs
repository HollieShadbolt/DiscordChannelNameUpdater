namespace DiscordChannelNameUpdater.Interfaces;

/// <summary>
/// A Discord bot instance for updating a channel name.
/// </summary>
public interface IDiscordChannelNameUpdater
{
    /// <summary>
    /// Run.
    /// </summary>
    /// <param name="cancellationToken"> The cancellation token to cancel operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task RunAsync(CancellationToken cancellationToken);
}