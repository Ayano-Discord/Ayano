using System.Text.RegularExpressions;
using Remora.Discord.API.Objects;
using Remora.Discord.API;
using System.Drawing;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Ayano.Core.Responders;

public class MentionSelfResponder : IResponder<IMessageCreate>
{
    private readonly IDiscordRestChannelAPI _discordRestChannelApi;
    private readonly IDiscordRestUserAPI _discordRestUserApi;

    public MentionSelfResponder(
        IDiscordRestUserAPI discordRestUserApi,
        IDiscordRestChannelAPI discordRestChannelApi)
    {
        _discordRestUserApi = discordRestUserApi;
        _discordRestChannelApi = discordRestChannelApi;
    }

    public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
    {
        var userResult = await _discordRestUserApi.GetCurrentUserAsync(ct);
        var avatarUrlResult = CDN.GetUserAvatarUrl(userResult.Entity);
        var embed = new Embed
        {
            Title = "Hello There!",
            Description = "Hello! I'm Ayano a multipurpose Discord bot written in C#!" +
                          "My prefixes in these server are `!`",
            Colour = Color.Crimson
        };

        var botMention = $"^<@!?{userResult.Entity.ID.Value}>$";

        if (!Regex.IsMatch(gatewayEvent.Content, botMention))
            return Result.FromSuccess();

        var result = await _discordRestChannelApi.CreateMessageAsync(
            gatewayEvent.ChannelID,
            embeds: new[] { embed }
            , ct: ct);

        return result.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(result.Error);
    }
}