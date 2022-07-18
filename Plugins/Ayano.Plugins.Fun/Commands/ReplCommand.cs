using System.Drawing;
using Polly;
using Remora.Commands.Attributes;
using Remora.Discord.Extensions.Embeds;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ayano.Core.Services.CodePaste;
using Ayano.Core.Services.Utilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using RemoraResult = Remora.Results.Result;

namespace Ayano.Plugins.Fun.Commands;

public class Result
{
    public object ReturnValue { get; set; }
    public string Exception { get; set; }
    public string Code { get; set; }
    public string ExceptionType { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public TimeSpan CompileTime { get; set; }
    public string ConsoleOut { get; set; }
    public string ReturnTypeName { get; set; }
}

[Group("repl")]
[Description("Execute & demonstrate code snippets.")]
public class ReplModule : CommandGroup
{
    private const int MaxFormattedFieldSize = 1000;
    private const string DefaultReplRemoteUrl = "http://localhost:31337/Eval";
    private string _replUrl { get; init; }
    private CodePasteService _pasteService { get; init; }
    private MessageContext Context { get; init; }
    private IHttpClientFactory _httpClientFactory { get; init; }
    private IDiscordRestChannelAPI ChannelApi { get; init; }

    public ReplModule(
        CodePasteService pasteService,
        MessageContext context,
        IHttpClientFactory httpClientFactory,
        IDiscordRestChannelAPI channelApi)
    {
        _pasteService = pasteService;
        _httpClientFactory = httpClientFactory;
        _pasteService = pasteService;
        _replUrl = DefaultReplRemoteUrl;
        Context = context;
        ChannelApi = channelApi;
    }

    [Command("exec", "eval", "e"), Description("Executes the given C# code and returns the result.")]
    public async Task<RemoraResult> ReplInvokeAsync(
        [Greedy]
        [Description("The code to execute.")]
            string code)
    {
        var messageLink = "https://canary.discord.com/channels/" + Context.GuildID + "/" + Context.ChannelID + "/" + Context.MessageID;
        
        var channel = ChannelApi.GetChannelAsync(Context.ChannelID).Result.Entity;
        
        if (!(channel.Type is ChannelType.GuildText))
        {
            var errorEmbed = new Embed
            {
                Title = "Repl Error",
                Description = "The REPL can only be executed in public guild channels.",
                Colour = Color.Red,
                Author = new EmbedAuthor(Context.User.Username,
                    IconUrl: CDN.GetUserAvatarUrl(Context.User).Entity.ToString()),
                Fields = new EmbedField[]
                {
                    new("Tried to execute", $"[this code]({messageLink})")
                }
            };

            await ChannelApi.CreateMessageAsync(Context.ChannelID, embeds: new[] { errorEmbed });
            return RemoraResult.FromError<string>("The REPL can only be executed in public guild channels.");
        }

        var compilingEmbed = new Embed
        {
            Title = "Executing code...",
            Author = new EmbedAuthor(Context.User.Username,
                IconUrl: CDN.GetUserAvatarUrl(Context.User).Entity.ToString()),
            Colour = Color.Orange,
            Description = $"Compiling and Executing [your code]({messageLink})..."
        };

        var message = await ChannelApi.CreateMessageAsync(Context.ChannelID, embeds: new[] { compilingEmbed });

        var content = FormatUtilities.BuildContent(code);

        // make it easier to trace calls back to discord if moderation or investigation needs to happen
        content.Headers.TryAddWithoutValidation("X-Ayano-DiscordUserId", Context.User.ID.ToString());

        HttpResponseMessage res;
        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.RetryOnTransientErrorPolicy);
            res = await client.PostAsync(_replUrl, content);

        }
        catch (IOException ex)
        {
            await ModifyOrSendErrorEmbed("Recieved an invalid response from the REPL service." +
                                         $"\n\n**Details:**\n{ex.Message}", message.Entity);
            return RemoraResult.FromError<string>("Recieved an invalid response from the REPL service." +
                                                   $"\n\n**Details:**\n{ex.Message}");
        }
        catch (Exception ex)
        {
            await ModifyOrSendErrorEmbed("An error occurred while sending a request to the REPL service. " +
                                         "This may be due to a StackOverflowException or exceeding the 30 second timeout." +
                                         $"\n\n**Details:**\n{ex.Message}", message.Entity);
            return RemoraResult.FromError<string>("An error occurred while sending a request to the REPL service. " +
                                                   "This may be due to a StackOverflowException or exceeding the 30 second timeout." +
                                                   $"\n\n**Details:**\n{ex.Message}");
        }

        if (!res.IsSuccessStatusCode & res.StatusCode != HttpStatusCode.BadRequest)
        {
            await ModifyOrSendErrorEmbed($"Status Code: {(int)res.StatusCode} {res.StatusCode}", message.Entity);
            return RemoraResult.FromError<string>($"Status Code: {(int)res.StatusCode} {res.StatusCode}");
        }

        var parsedResult = JsonConvert.DeserializeObject<Result>(await res.Content.ReadAsStringAsync());

        var embed = await BuildEmbedAsync(parsedResult);

        await ChannelApi.DeleteMessageAsync(Context.ChannelID, Context.MessageID);
        
        return (RemoraResult)await ChannelApi.CreateMessageAsync(Context.ChannelID, embeds: new[] { embed });
    }

    private async Task ModifyOrSendErrorEmbed(string error, IMessage message)
    {
        var messageLink = "https://canary.discord.com/channels/" + Context.GuildID + "/" + Context.ChannelID + "/" + Context.MessageID;
        
        var embed = new Embed
        {
            Title = "Repl Error",
            Description = error,
            Colour = Color.Red,
            Author = new EmbedAuthor(Context.User.Username,
                IconUrl: CDN.GetUserAvatarUrl(Context.User).Entity.ToString()),
            Fields = new EmbedField[]
            {
                new("Tried to execute", $"[this code]({messageLink})")
            }
        };

        await ChannelApi.EditMessageAsync(Context.ChannelID, message.ID, embeds: new[] { embed });
    }

    private async Task<Embed> BuildEmbedAsync(Result parsedResult)
    {
        var returnValue = parsedResult.ReturnValue?.ToString() ?? " ";
        var consoleOut = parsedResult.ConsoleOut;
        var status = string.IsNullOrEmpty(parsedResult.Exception) ? "Success" : "Failure";
        
        var input = Regex.Replace(
            parsedResult.Exception, 
            "^", 
            "- ", 
            RegexOptions.Multiline
        ).TruncateTo(MaxFormattedFieldSize);

        var embed = new Embed
        {
            Title = $"REPL Result: {status}",
            Colour = string.IsNullOrEmpty(parsedResult.Exception) ? Color.Green : Color.Red,
            Author = new EmbedAuthor(Context.User.Username,
                IconUrl: CDN.GetUserAvatarUrl(Context.User).Entity.ToString()),
            Footer = new EmbedFooter(
                $"Compile: {parsedResult.CompileTime.TotalMilliseconds:F}ms | Execution: {parsedResult.ExecutionTime.TotalMilliseconds:F}ms"),
            Description = FormatOrEmptyCodeblock(parsedResult.Code, "cs"),
            Fields = new EmbedField[]
            {
                string.IsNullOrWhiteSpace(consoleOut) ? null : new ("Console Output", consoleOut),
                string.IsNullOrWhiteSpace(parsedResult.Exception) ? null : new (
                    "Exception", 
                    $"```{input}\n```"
                    ),
                new ($"Result: {parsedResult.ReturnTypeName}", FormatOrEmptyCodeblock(returnValue.TruncateTo(MaxFormattedFieldSize), "json"))
            }
        };

        // if (!string.IsNullOrWhiteSpace(consoleOut))
        // {
        //     embed.AddField(a => a.WithName("Console Output")
        //                          .WithValue(Format.Code(consoleOut.TruncateTo(MaxFormattedFieldSize), "txt")));
        //     await embed.UploadToServiceIfBiggerThan(consoleOut,  MaxFormattedFieldSize, _pasteService);
        // }

        // if (!string.IsNullOrWhiteSpace(parsedResult.Exception))
        // {
        //     var diffFormatted = Regex.Replace(parsedResult.Exception, "^", "- ", RegexOptions.Multiline);
        //     embed.AddField(a => a.WithName($"Exception: {parsedResult.ExceptionType}")
        //                          .WithValue(Format.Code(diffFormatted.TruncateTo(MaxFormattedFieldSize), "diff")));
        //     await embed.UploadToServiceIfBiggerThan(diffFormatted, MaxFormattedFieldSize, _pasteService);
        // }

        return embed;
    }

    private static string FormatOrEmptyCodeblock(string input, string language)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "```\n```";
        return $"```{language}\n{input}\n```";
    }
}

public static class Extensions
{
    public static string TruncateTo(this string str, int length)
    {
        return str.Length < length ? str : str.Substring(0, length);
    }
}
