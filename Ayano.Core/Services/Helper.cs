using System.Drawing;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Core.Services;

public class Helper
{
    public Helper(IDiscordRestGuildAPI restGuildApi)
    {
        RestGuildApi = restGuildApi;
    }

    private static IDiscordRestGuildAPI RestGuildApi { get; set; }

    public static bool LinkIsImage(string url)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            url,
            @"(https?:\/\/[^ ]*\.(?:gif|png|jpg|jpeg))"
            );
    }

    public async Task<Color> GetUserColourAsync(MessageContext context)
    {
        var userRole = await RestGuildApi.GetGuildMemberAsync(context.GuildID.Value, context.User.ID);
        var guildRoles = await RestGuildApi.GetGuildRolesAsync(context.GuildID.Value);

        foreach (var i in guildRoles.Entity)
            if (i.ID == userRole.Entity.Roles.Last())
                return i.Colour;

        return Color.Crimson;
    }
}