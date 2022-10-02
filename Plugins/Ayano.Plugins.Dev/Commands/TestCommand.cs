using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using System.Net.WebSockets;
using System.Text;

namespace Ayano.Plugins.Dev.Commands;

public class TestCommand : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly MessageContext _context;

    public TestCommand(
        IDiscordRestChannelAPI channelApi,
        MessageContext context
    )
    {
        _channelApi = channelApi;
        _context = context;
    }

    [Command("lavalink")]
    [Description("Some hacky stuff")]
    public async Task<Result> Command()
    {
        await Init();

        var result = await _channelApi.CreateMessageAsync(_context.ChannelID, "Done!");
        return result.IsSuccess ? Result.FromSuccess() : Result.FromError(result.Error);
    }

    private async Task Init()
    {
        var ws = new ClientWebSocket();

        ws.Options.SetRequestHeader("Authorization", "password123");
        ws.Options.SetRequestHeader("User-Id", "762976674659696660");
        ws.Options.SetRequestHeader("Client-Name", "Onii.Remora.Lavalink");
        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

        var playString = @"{
        'op': 'play',
        'guildId': '852094131047104593',
        'track': 'https://www.youtube.com/watch?v=D3p10NBSPCU',
        'startTime': '60000',
        'endTime': '120000',
        'volume': '100',
        'noReplace': false,
        'pause': false
        }
        ";

        await ws.ConnectAsync(new Uri("wss://localhost:2333/"), CancellationToken.None);

        await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(playString)), WebSocketMessageType.Text, true,
            CancellationToken.None);

        var result = ws.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), CancellationToken.None).Result;

        if (result.MessageType == WebSocketMessageType.Text)
        {
            Console.WriteLine(Encoding.UTF8.GetString(new byte[1024], 0, result.Count));
        }

        await _channelApi.CreateMessageAsync(_context.ChannelID, Encoding.UTF8.GetString(new byte[1024], 0, result.Count));

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

        return;
    }
}