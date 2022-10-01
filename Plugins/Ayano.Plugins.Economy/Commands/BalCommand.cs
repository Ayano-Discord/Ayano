using System.Globalization;
using System.Drawing;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using System.ComponentModel;
using Remora.Results;
using Ayano.Core.Services;
using Humanizer;

namespace Ayano.Plugins.Economy.Commands;
public class BalanceCommand: CommandGroup
{
    private MessageContext Context { get; init; }
    private IDiscordRestChannelAPI ChannelApi { get; init; }
    private Database _db { get; init; }

    public BalanceCommand (
        MessageContext context,
        IDiscordRestChannelAPI channelAPI,
        Database db
    )
    {
        Context = context;
        ChannelApi = channelAPI;
        _db = db;
    }

    [Command("balance", "bal")]
    [Description("Check your bank and wallet balance.")]
    public async Task<IResult> Balance()
    {
        var bal = await _db.GetBalance(Context.User.ID);
        var embed = new Embed
        {
            Title = "Balance",
            Description = $"Wallet: {bal}",
            Colour = Color.AliceBlue
        };

        var msg = await ChannelApi.CreateMessageAsync(Context.ChannelID, embeds: new[] { embed });
        return msg.IsSuccess ? Result.FromSuccess() : Result.FromError(msg.Error);; 
    }
}