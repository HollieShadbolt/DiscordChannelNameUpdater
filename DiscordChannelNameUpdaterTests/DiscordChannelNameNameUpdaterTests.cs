using Moq;

namespace DiscordChannelNameUpdaterTests;

[TestFixture]
public static class DiscordChannelNameNameUpdaterTests
{
    [Test]
    public static void RunAsync_Test()
    {
        // Arrange
        var mockDiscord = new Mock<Discord.Interfaces.IDiscord>();

        var guildId = Guid.NewGuid().ToString();

        var userId = Guid.NewGuid().ToString();

        var channelId = Guid.NewGuid().ToString();

        var getUserVoiceStateCount = 0;

        var getUserVoiceStateChannelIdAsyncParams = new Discord.Params.GetUserVoiceStateChannelIdAsyncParams
        {
            GuildId = guildId,
            UserId = userId
        };

        var cancellationTokenSource = new CancellationTokenSource();

        mockDiscord
            .Setup(discord =>
                discord.GetUserVoiceStateChannelIdAsync(getUserVoiceStateChannelIdAsyncParams,
                    cancellationTokenSource.Token))
            .ReturnsAsync(() => getUserVoiceStateCount++ < 3 ? null : channelId);

        var channelNameIfConnected = Guid.NewGuid().ToString();

        var channelNameIfDisconnected = Guid.NewGuid().ToString();

        var modifyChannelNameAsyncParamsChannelNameIfDisconnected = new Discord.Params.ModifyChannelNameAsyncParams
        {
            ChannelId = channelId,
            Name = channelNameIfDisconnected
        };

        mockDiscord
            .Setup(discord =>
                discord.ModifyChannelNameAsync(modifyChannelNameAsyncParamsChannelNameIfDisconnected,
                    cancellationTokenSource.Token))
            .ThrowsAsync(new TaskCanceledException());

        var mockConfig = new Mock<DiscordChannelNameUpdater.Interfaces.IConfig>();

        mockConfig.SetupGet(config => config.GuildId).Returns(guildId);
        mockConfig.SetupGet(config => config.UserId).Returns(userId);
        mockConfig.SetupGet(config => config.ChannelId).Returns(channelId);
        mockConfig.SetupGet(config => config.ChannelNameIfDisconnected).Returns(channelNameIfConnected);
        mockConfig.SetupGet(config => config.ChannelNameIfConnected).Returns(channelNameIfDisconnected);

        var mockDelayHandler = new Mock<HttpRequestMessageHandler.Interfaces.IDelayHandler>();

        var channelNameUpdater = new DiscordChannelNameUpdater.DiscordChannelNameNameUpdater(
            mockDiscord.Object,
            mockConfig.Object,
            mockDelayHandler.Object);

        // Act
        Assert.ThrowsAsync<TaskCanceledException>(() => channelNameUpdater.RunAsync(cancellationTokenSource.Token));

        // Assert
        mockDiscord.Verify(
            discord => discord.GetUserVoiceStateChannelIdAsync(getUserVoiceStateChannelIdAsyncParams,
                cancellationTokenSource.Token), Times.Exactly(4));

        mockDiscord.Verify(
            discord => discord.ModifyChannelNameAsync(modifyChannelNameAsyncParamsChannelNameIfDisconnected,
                cancellationTokenSource.Token),
            Times.Exactly(1));

        var modifyChannelNameAsyncParamsChannelNameIfConnected = new Discord.Params.ModifyChannelNameAsyncParams
        {
            ChannelId = channelId,
            Name = channelNameIfConnected
        };

        mockDiscord.Verify(
            discord => discord.ModifyChannelNameAsync(modifyChannelNameAsyncParamsChannelNameIfConnected,
                cancellationTokenSource.Token), Times.Exactly(1));

        mockDiscord.VerifyNoOtherCalls();

        mockDelayHandler.Verify(delayHandler => delayHandler.Delay(300_000, cancellationTokenSource.Token),
            Times.Exactly(1));

        mockDelayHandler.VerifyNoOtherCalls();
    }
}