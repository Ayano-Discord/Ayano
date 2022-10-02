﻿using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;

namespace Ayano.Plugins.Dev.Commands;

public class EvalCommand : CommandGroup
{
    private const string _usings = @"
System
System.Collections
System.Collections.Generic
System.ComponentModel
System.Drawing
System.Linq
System.Reflection
System.Runtime.CompilerServices
System.Text.RegularExpressions
System.Threading.Tasks
Remora.Commands.Attributes
Remora.Commands.Groups
Remora.Discord.API.Abstractions.Objects
Remora.Discord.API.Abstractions.Rest
Remora.Discord.API.Objects
Remora.Discord.Commands.Contexts
Remora.Rest.Core
Remora.Results
MediatR
Microsoft.EntityFrameworkCore
Remora.Rest.Core
Remora.Results
System.Text
Microsoft.Extensions.Logging
Ayano.Core.Services
Ayano
Ayano.Plugins
";

    private static readonly IEmbed _evaluatingEmbed = new Embed
    {
        Title = "Evaluating. Please wait.",
        Colour = Color.HotPink
    };

    private readonly IDiscordRestChannelAPI _channels;
    private readonly MessageContext _context;
    private readonly IDiscordRestGuildScheduledEventAPI _events;
    private readonly IDiscordRestGuildAPI _guilds;

    private readonly IServiceProvider _services;

    private readonly IDiscordRestUserAPI _users;

    public EvalCommand(MessageContext context, IDiscordRestChannelAPI channels, IDiscordRestUserAPI users,
        IDiscordRestGuildAPI guilds, IDiscordRestGuildScheduledEventAPI events, IServiceProvider services)
    {
        _context = context;
        _channels = channels;
        _users = users;
        _guilds = guilds;
        _events = events;
        _services = services;
    }

    [Command("eval")]
    [Description("Evaluates code.")]
    public async Task<Result> EvalCS([Greedy] string _)
    {
        var cs = Regex.Replace(_context.Message.Content.Value,
            @"^(?:\S{0,24}?eval ? \n?)((?:(?!\`\`\`)(?<code>[\S\s]+))|(?:(?:\`\`\`cs|csharp\n)(?<code>[\S\s]+)\n?\`\`\`$))",
            "$1", RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.Multiline);

        var messageResult = await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { _evaluatingEmbed });

        if (!messageResult.IsDefined(out var msg))
            return Result.FromError(messageResult.Error!);

        try
        {
            var globals = new EvalVariables
            {
                UserID = _context.User.ID,
                GuildID = _context.GuildID.IsDefined(out var guildID) ? guildID : default,
                ChannelID = _context.ChannelID,
                MessageID = _context.MessageID,
                ReplyMessageID = _context.Message.ReferencedMessage.IsDefined(out var reply) ? reply.ID : default,

                Services = _services,

                Users = _users,
                Guilds = _guilds,
                Channels = _channels,
                ScheduledEvents = _events
            };

            var sopts = ScriptOptions.Default;
            sopts = sopts.AddImports(_usings.Split('\n',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            var asm = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(xa =>
                    !xa.IsDynamic &&
                    !string.IsNullOrWhiteSpace(xa.Location));

            sopts = sopts.WithReferences(asm);

            Script<object> script = CSharpScript.Create(cs, sopts, typeof(EvalVariables));

            var evalResult = await script.RunAsync(globals);

            if (string.IsNullOrEmpty(evalResult.ReturnValue?.ToString()))
            {
                await _channels.EditMessageAsync(_context.ChannelID, msg.ID, "The evaluation returned null or void.",
                    Array.Empty<IEmbed>());
                return Result.FromSuccess();
            }

            if (evalResult.ReturnValue is IEmbed embed)
            {
                var edit = await _channels.EditMessageAsync(_context.ChannelID, msg.ID, embeds: new[] { embed });

                if (!edit.IsSuccess)
                {
                    await _channels.EditMessageAsync(_context.ChannelID, msg.ID,
                        "Failed to edit message.\n" + edit.Error);
                    return Result.FromError(edit.Error);
                }
            }

            var returnResult = GetHumanFriendlyResultString(evalResult.ReturnValue);

            var returnEmbed = new Embed
            {
                Title = "Evaluation Result",
                Description = returnResult ?? "Something went horribly wrong help",
                Colour = Color.MidnightBlue
            };

            await _channels.EditMessageAsync(_context.ChannelID, msg.ID, embeds: new[] { returnEmbed });
        }
        catch (Exception ex)
        {
            var exEmbed = new Embed
            {
                Title = "Eval Error!",
                Description = $"**{ex.GetType()}**: {ex.Message.Split('\n')[0]}",
                Colour = Color.Firebrick
            };

            await _channels.EditMessageAsync(_context.ChannelID, msg.ID, embeds: new[] { exEmbed });
        }

        return Result.FromSuccess();
    }

    private string? GetHumanFriendlyResultString(object? result)
    {
        if (result is null)
            return "null";

        var type = result.GetType();

        var returnResult = result.ToString();

        if (type.IsGenericType && type.GetGenericTypeDefinition() is { } gt &&
            (gt.IsAssignableTo(typeof(IEnumerable<>)) || gt.IsAssignableTo(typeof(IList))))
        {
            returnResult = "{ " + typeof(CollectionExtensions)
                .GetMethod("Join", BindingFlags.Static | BindingFlags.Public)!
                .MakeGenericMethod(type.GetGenericArguments()[0])
                .Invoke(null, new[] { result, ", " }) + " }";
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableTo(typeof(Result<>)))
        {
            var error =
                type.GetProperty(nameof(Result.Error), BindingFlags.Public | BindingFlags.Instance)!.GetValue(result)!;
            var success = type.GetProperty(nameof(Result.IsSuccess), BindingFlags.Public | BindingFlags.Instance)!
                .GetValue(result)!;
            var entity = type.GetProperty(nameof(Result<object>.Entity), BindingFlags.Public | BindingFlags.Instance)!
                .GetValue(result)!;

            returnResult = $"Result<{type.GenericTypeArguments[0].Name}>:\n" +
                           $"\u200b\tIsSuccess: {success}\n" +
                           $"\u200b\tEntity: {GetHumanFriendlyResultString(entity)}\n" + // Just in case the entity itself is a result or a collection
                           $"\u200b\tError: {error}";
        }
        else if (type.IsAssignableTo(typeof(IList)))
        {
            returnResult = "{ " + typeof(CollectionExtensions)
                .GetMethod("Join", BindingFlags.Static | BindingFlags.Public)!
                .MakeGenericMethod(type.GetElementType()!)
                .Invoke(null, new[] { result, ", " })! + " }";
        }
        else if (type.IsAssignableTo(typeof(IResult)))
        {
            var res = (IResult)result;

            returnResult = $"Result ({type.Name}):\n" +
                           $"\tIsSuccess: {res.IsSuccess}\n" +
                           $"\tError: {res.Error}";
        }

        return returnResult;
    }

    public record EvalVariables
    {
        public Snowflake UserID { get; init; }
        public Snowflake GuildID { get; init; }
        public Snowflake ChannelID { get; init; }
        public Snowflake MessageID { get; init; }
        public Snowflake ReplyMessageID { get; init; }

        public IServiceProvider Services { get; init; }

        public IDiscordRestUserAPI Users { get; init; }
        public IDiscordRestGuildAPI Guilds { get; init; }
        public IDiscordRestChannelAPI Channels { get; init; }
        public IDiscordRestGuildScheduledEventAPI ScheduledEvents { get; init; }
    }
}