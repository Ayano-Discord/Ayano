using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using System.ComponentModel;
using Remora.Results;
using xFunc.Maths;

namespace Ayano.Plugins.Fun;
public class MathsCommands : CommandGroup
{
    private MessageContext Context { get; init; }
    private IDiscordRestChannelAPI ChannelApi { get; init; }
    private Processor _processor { get; init; }

    public MathsCommands (
        MessageContext context,
        IDiscordRestChannelAPI channelApi,
        Processor processor)
    {
        Context = context;
        ChannelApi = channelApi;
        _processor = processor;
    }

    [Command("maths")]
    [Description("Evaluate a maths expression.")]
    public async Task<IResult> Maths([Description("Expression")]string expression)
    {

        var exp = _processor.Parse(expression);

        var embed = new Embed() {
            Title = "Maths",
            Fields = new[] {
                new EmbedField("Expression", expression),
                new EmbedField("Result", exp.Execute().ToString()!)
            }
        };

        var answer = exp.Execute();

        return await ChannelApi.CreateMessageAsync(Context.ChannelID, embeds: new[] {embed});
    }
}