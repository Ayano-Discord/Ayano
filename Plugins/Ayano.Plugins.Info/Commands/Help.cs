// using System.ComponentModel;
// using System.Text;
// using Remora.Commands.Attributes;
// using Remora.Commands.Groups;
// using Remora.Commands.Signatures;
// using Remora.Commands.Trees;
// using Remora.Commands.Trees.Nodes;
// using Remora.Discord.API.Abstractions.Objects;
// using Remora.Discord.API.Abstractions.Rest;
// using Remora.Discord.API.Objects;
// using Remora.Discord.Commands.Contexts;
// using Remora.Discord.Rest.API;
// using Remora.Rest.Ayano.Core;
// using Remora.Results;
// using SixLabors.ImageSharp;
//
// namespace Ayano.Plugins.Info;
//
// public class Help : CommandGroup
//     {
//         private IDiscordRestChannelAPI RestChannelAPI { get; init; }
//         private MessageContext CommandContext { get; init; }
//         private CommandTree CommandTree { get; init; }
//         private IDiscordRestGuildAPI RestGuildAPI { get; init; }
//
//         public Help(IDiscordRestChannelAPI restChannelAPI, IDiscordRestGuildAPI restGuildApi, MessageContext commandContext, CommandTree commandTree, IDiscordRestGuildAPI restGuildAPI, IDiscordRestUserAPI restUserAPI)
//         {
//
//             RestChannelAPI = restChannelAPI;
//             RestGuildAPI = restGuildAPI;
//             CommandContext = commandContext;
//             CommandTree = commandTree;
//         }
//         
//         private static string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
//         private static Random random = new Random();
//
//         private static string GenerateNonce(int length)
//         {
//             var nonceString = new StringBuilder();
//             for (int i = 0; i < length; i++)
//             {
//                 nonceString.Append(validChars[random.Next(0, validChars.Length - 1)]);
//             }
//
//             return nonceString.ToString();
//         }
//
//         [Command("help"), Description("Sends the help message.")]
//         public async Task<Result<IMessage>> HelpAsync()
//         {
//             List<IMessage> pages = new();
//             List<EmbedField> embedFields = new();
//             for (int i = 0; i < CommandTree.Root.Children.Count; i++)
//             {
//                 IChildNode command = CommandTree.Root.Children[i];
//                 embedFields.Add(new(command.Key, "A description which I do not have access too."));
//
//                 if (embedFields.Count == 25 || (i + 1) == CommandTree.Root.Children.Count)
//                 {
//                     // I hate this.
//                     pages.Add(new Message(
//                             Snowflake.CreateTimestampSnowflake(DateTime.UtcNow),
//                             CommandContext.ChannelID,
//                             CommandContext.User,
//                             "", 
//                             DateTime.UtcNow,
//                             null, 
//                             false, 
//                             false, 
//                             Array.Empty<Snowflake>(), 
//                             Array.Empty<IChannelMention>(), 
//                             Array.Empty<IAttachment>(),
//                             new[] { 
//                                 new Embed(
//                                     Author: new EmbedAuthor(
//                                         $"MeiChan's Help Menu - Page {pages.Count + 1}",
//                                         IconUrl: "https://cdn.discordapp.com/avatars/762976674659696660/a9a375a67ea7908fb79a65405c011201.png?size=1024"
//                                         ),
//                                     Fields: embedFields.ToArray(),
//                                     Colour: System.Drawing.Color.Crimson
//                                 )
//                             }, 
//                             new Optional<IReadOnlyList<IReaction>>(), 
//                             new Optional<string>(),
//                             false, 
//                             new Optional<Snowflake>(),
//                             Type: MessageType.Default
//                         )
//                     );
//                     embedFields.Clear();
//                 }
//
//             }
//             return await RestChannelAPI.CreateMessageAsync(CommandContext.ChannelID, embeds: new(pages[0].Embeds));
//         }
//
//         [Command("help"), Description("Sends the help message.")]
//         public async Task<Result<IMessage>> HelpAsync([Greedy] string? commandName)
//         {
//             StringBuilder stringBuilder = new();
//             List<IMessage> pages = new();
//             foreach (BoundCommandNode? commandNode in CommandTree.Search(commandName, null, new TreeSearchOptions(StringComparison.InvariantCultureIgnoreCase)))
//             {
//                 if (commandNode.Node.Shape.Parameters.Count != 0)
//                 {
//                     foreach (IParameterShape parameter in commandNode.Node.Shape.Parameters)
//                     {
//                         stringBuilder.Append("`[--");
//                         stringBuilder.Append(parameter.HintName);
//                         if (parameter.DefaultValue != null)
//                         {
//                             stringBuilder.Append('=');
//                             stringBuilder.Append(parameter.DefaultValue);
//                         }
//                         stringBuilder.Append("]` - ");
//                         stringBuilder.AppendLine(parameter.Description);
//                     }
//                 }
//                 else
//                 {
//                     stringBuilder.Append("None.");
//                 }
//
//                 Embed embed = new(
//                     // Title: $"MeiChan's Help Menu - Page {pages.Count + 1}",
//                     Colour: System.Drawing.Color.Crimson,
//                     Description: commandNode.Node.Shape.Description,
//                     Fields: new EmbedField[] {
//                         new("Aliases", commandNode.Node.Aliases.Count == 0 ? "None." : string.Join(", ", commandNode.Node.Aliases)),
//                         new("Parameters", stringBuilder.ToString())
//                     },
//                     Author: new EmbedAuthor(
//                         $"MeiChan's Help Menu - Page {pages.Count + 1}",
//                         IconUrl: "https://cdn.discordapp.com/avatars/762976674659696660/a9a375a67ea7908fb79a65405c011201.png?size=1024"
//                     ),
//                     Footer: new EmbedFooter("Type %help <command_or_category> for more info on a command or category.")
//                 );
//
//                 // I double hate this.
//                 pages.Add(new Message(
//                     Snowflake.CreateTimestampSnowflake(DateTime.UtcNow),
//                     CommandContext.ChannelID,
//                     CommandContext.User,
//                     "", 
//                     DateTime.UtcNow,
//                     null, 
//                     false, 
//                     false, 
//                     Array.Empty<Snowflake>(), 
//                     Array.Empty<IChannelMention>(), 
//                     Array.Empty<IAttachment>(),
//                     new[] { embed }, 
//                     new Optional<IReadOnlyList<IReaction>>(), 
//                     new Optional<string>(),
//                     false, 
//                     new Optional<Snowflake>(),
//                     Type: MessageType.Default
//                     )
//                 );
//             }
//
//             return await RestChannelAPI.CreateMessageAsync(CommandContext.ChannelID, embeds: new(pages[0].Embeds));
//         }
//     }

