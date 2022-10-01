using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using System.ComponentModel;
using Remora.Results;
using Ayano.Core.Services;
using Humanizer;

namespace Ayano.Plugins.Economy.Commands;
public class WorkCommand: CommandGroup
{
    private MessageContext Context { get; init; }
    private IDiscordRestChannelAPI ChannelApi { get; init; }
    private Database _db { get; init; }

    public WorkCommand (
        MessageContext context,
        IDiscordRestChannelAPI channelAPI,
        Database db
    )
    {
        Context = context;
        ChannelApi = channelAPI;
        _db = db;
    }

    private string[] jobs = new[] {
        "Doctor",
        "Dishwasher",
        "Memer",
        "YouTuber",
        "Developer",
        "Musician",
        "Professional sleeper",
        "Teacher",
        "Scientist",
        "Twitch Streamer",
        "StickAnimator",
        "Strict Math Teacher",
        "Tik Toker",
        "Miner", 
        "Bartender", 
        "Cashier", 
        "Cleaner", 
        "Drugdealer",
        "Assistant", 
        "Nurse",
        "Accountants", 
        "Security Guard", 
        "Sheriff", 
        "Lawyer",
        "Electrician", 
        "Singer", 
        "Dancer"
    };

    [Command("work")]
    [Description("Work for some money.")]
    public async Task<IResult> Work()
    {

        // var cooldown = await _db.GetCooldown(Context.User.ID, "work");
        // var i = TimeSpan.FromMilliseconds((double)cooldown).Humanize(3, countEmptyUnits:true);

        // if (0 < DateTime.Now.Millisecond) {
        //     return await ChannelApi.CreateMessageAsync(Context.ChannelID, "You are tired right now. Come back after {} to work again.");
        // }

        var amount = (UInt64)Math.Floor(new Random().NextDouble() * 1500) + 100;

        var bal = await _db.GetBalance(Context.User.ID);

        await _db.SetBalance(Context.User.ID, (ulong)bal + amount);

        var job = jobs[new Random().Next(jobs.Length)];

        var msg = await ChannelApi.CreateMessageAsync(Context.ChannelID, $"You worked as **{job}** and earned **{amount}** 💸. Now you have **${bal + amount}** 💸.");

        return msg.IsSuccess ? Result.FromSuccess() : Result.FromError(msg.Error);; 
    }
}