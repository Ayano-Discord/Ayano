using System.Diagnostics;
using System.Drawing;
using Ayano.Core.Services.Cache;
using Humanizer;
using Humanizer.Localisation;
using Octokit;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Ayano.Plugins.Info.Commands;

public class AboutCommand : CommandGroup
{
    private GitHubClient GithubClient { get; init; }
    private IDiscordRestChannelAPI ChannelApi { get; init; }
    private IDiscordRestGuildAPI GuildApi { get; init; }

    private ICacheService CacheService { get; init; }

    private MessageContext Context { get; init; }

    public AboutCommand(GitHubClient client, IDiscordRestChannelAPI channelApi, MessageContext context, IDiscordRestGuildAPI guildApi, ICacheService cacheService)
    {
        GithubClient = client;
        ChannelApi = channelApi;
        Context = context;
        GuildApi = guildApi;
        CacheService = cacheService;
    }

    public string GetLatestCommits(int count = 3)
    {
        var commits = GithubClient.Repository.Commit.GetAll(443498716).Result!;

        var commitString = "";

        for (var i = 0; i < count; i++)
        {
            var timestamp = commits[i].Commit.Author.Date.ToUnixTimeSeconds();
            var commitUrl = "https://github.com/Ayano-Discord/Ayano/commit/" + commits[i].Sha;

            commitString += $"[`{commits[i].Sha.Substring(0, 6)}`]({commitUrl}) - {commits[i].Commit.Message} (<t:{timestamp}:R>)\n";
        }

        return commitString;
    }

    [Command("about")]
    public async Task<Result> Command()
    {
        using var process = Process.GetCurrentProcess();

        var statsString = @$"
            Ayano is running on `1` shard.
            Serving `{CacheService.Get<int>("global:guildCount").Some()}` servers (`{CacheService.Get<int>("global:channel:totalChannelCount").Some()}` Channels).
            For a total of `{CacheService.Get<int>("global:userCount").Some()}` users.
            ";

        var embed = new Embed
        {
            Author = new EmbedAuthor("Ayano"),
            Title = "About Ayano",
            Fields = new EmbedField[]
            {
                new ("Who is Ayano?", "Ayano is a discord bot inspired by many others. Its aim is to ease moderation and performing tasks more efficiently than users, while maintaining a funny personality through its variety of commands.", false),
                new ("Stats", statsString),
                new ("Latest Commits", GetLatestCommits(), false)
            },
            Colour = Color.Crimson,
            Footer = new EmbedFooter($"Shard ID 1 • {DateTimeOffset.UtcNow.Subtract(process.StartTime).Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day)}")
        };
        var result = await ChannelApi.CreateMessageAsync(Context.ChannelID, embeds: new[] { embed });
        return result.IsSuccess ? Result.FromSuccess() : Result.FromError(result.Error);
    }
}