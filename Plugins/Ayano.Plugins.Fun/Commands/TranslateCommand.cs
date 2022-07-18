using System.ComponentModel;
using System.Drawing;
using GTranslate.Translators;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using RemoraResult = Remora.Results.Result;

namespace Ayano.Plugins.Fun.Commands;

public class TranslateCommand : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly MessageContext _context;

    public TranslateCommand(
        IDiscordRestChannelAPI channelApi,
        MessageContext context
    )
    {
        _channelApi = channelApi;
        _context = context;
    }

    [Command("translate", "tr")]
    [Description("Translates text from one language to another")]
    public async Task<RemoraResult> Command(string to, string text, string? from = null)
    {
        var preparingEmbed = new Embed
        {
            Title = "Writing random translations...",
            Image = new Optional<IEmbedImage>(new EmbedImage("https://c.tenor.com/B_25bFobJsMAAAAC/capoo-bug-cat.gif"))
        };


        var translator = new AggregateTranslator();
        var translateResult = await translator.TranslateAsync(text, to, from);

        var message =
            await _channelApi.CreateMessageAsync(_context.ChannelID, embeds: new[] { preparingEmbed });

        var translatedEmbed = new Embed
        {
            Title = "Results",
            Fields = new[]
            {
                new EmbedField("Original", text, false),
                new EmbedField("Translated", translateResult.Translation, false)
            },
            Footer = new EmbedFooter($"{translateResult.SourceLanguage.Name} → {translateResult.TargetLanguage.Name}"),
            Colour = Color.Crimson
        };

        await Task.Delay(1500);

        var result = await _channelApi.EditMessageAsync(_context.ChannelID, message.Entity.ID, string.Empty, new[] { translatedEmbed });
        return result.IsSuccess ? RemoraResult.FromSuccess() : RemoraResult.FromError(result.Error);
    }
}