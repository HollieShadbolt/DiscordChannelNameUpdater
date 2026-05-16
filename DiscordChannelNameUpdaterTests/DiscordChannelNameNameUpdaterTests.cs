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

        var channelNameIfFalse = Guid.NewGuid().ToString();

        var channelNameIfTrue = Guid.NewGuid().ToString();

        var modifyChannelNameAsyncParamsChannelNameIfTrue = new Discord.Params.ModifyChannelNameAsyncParams
        {
            ChannelId = channelId,
            Name = channelNameIfTrue
        };

        mockDiscord
            .Setup(discord =>
                discord.ModifyChannelNameAsync(modifyChannelNameAsyncParamsChannelNameIfTrue,
                    cancellationTokenSource.Token))
            .ThrowsAsync(new TaskCanceledException());

        var mockConfig = new Mock<DiscordChannelNameUpdater.Interfaces.IConfig>();

        mockConfig.SetupGet(config => config.GuildId).Returns(guildId);
        mockConfig.SetupGet(config => config.UserId).Returns(userId);
        mockConfig.SetupGet(config => config.ChannelId).Returns(channelId);
        mockConfig.SetupGet(config => config.ChannelNameIfFalse).Returns(channelNameIfFalse);
        mockConfig.SetupGet(config => config.ChannelNameIfTrue).Returns(channelNameIfTrue);

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
            discord => discord.ModifyChannelNameAsync(modifyChannelNameAsyncParamsChannelNameIfTrue,
                cancellationTokenSource.Token),
            Times.Exactly(1));

        var modifyChannelNameAsyncParamsChannelNameIfFalse = new Discord.Params.ModifyChannelNameAsyncParams
        {
            ChannelId = channelId,
            Name = channelNameIfFalse
        };

        mockDiscord.Verify(
            discord => discord.ModifyChannelNameAsync(modifyChannelNameAsyncParamsChannelNameIfFalse,
                cancellationTokenSource.Token), Times.Exactly(1));

        mockDiscord.VerifyNoOtherCalls();

        mockDelayHandler.Verify(delayHandler => delayHandler.Delay(300_000, cancellationTokenSource.Token),
            Times.Exactly(1));

        mockDelayHandler.VerifyNoOtherCalls();
    }
}