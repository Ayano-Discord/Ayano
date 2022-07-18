using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Humanizer;
using Humanizer.Localisation;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Gateway;
using Remora.Results;

namespace Ayano.Plugins.Info;

public class BotStats : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channels;
    private readonly ICommandContext _context;
    private readonly DiscordGatewayClient _gateway;
    private readonly IShardIdentification _shard;

    public BotStats
    (
        ICommandContext context,
        IDiscordRestChannelAPI channels,
        IShardIdentification shard,
        DiscordGatewayClient gateway
    )
    {
        _context = context;
        _channels = channels;
        _shard = shard;
        _gateway = gateway;
    }
    
    private async Task<string> GetCpuUsageForProcess()
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        await Task.Delay(500);
    
        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        return $"{Math.Round(cpuUsageTotal * 100, 2)}%";
    }

    [Command("botstats", "bs", "botinfo")]
    [Description("Get the current stats for Ayano")]
    public async Task<IResult> GetBotStatsAsync()
    {
        using var process = Process.GetCurrentProcess();

        var heapMemory = $"{GC.GetTotalMemory(true) / 1024 / 1024:n0} MB";

        var embed = new Embed
        {
            Title = $"Stats (Shard {_shard.ShardID + 1}):",
            Colour = Color.Gold,
            Fields = new EmbedField[]
            {
                new("Latency:", $"{_gateway.Latency.TotalMilliseconds:n0} ms", true),
                // new("Guilds:", guilds.ToString(), true),
                // new("Members:", members.ToString(), true),
                new("Memory:", heapMemory, true),
                new("CPU:", await GetCpuUsageForProcess(), true),
                new("Threads:", $"{ThreadPool.ThreadCount}", true),
                new("Uptime:",
                    $"{DateTimeOffset.UtcNow.Subtract(process.StartTime).Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day)}",
                    true)
            }
        };

        return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
    }
}