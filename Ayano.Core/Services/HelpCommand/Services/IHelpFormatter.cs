﻿using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;

namespace Ayano.Services.HelpCommand.Services;

public interface IHelpFormatter
{
    /// <summary>
    ///     Creates an embed for a help screen of a single command.
    /// </summary>
    /// <param name="command">The command that was found.</param>
    /// <returns>An embed displaying relevant information about the command.</returns>
    /// <remarks>
    ///     This should never be invoked with a group node, as the implementation may not properly handle it.
    ///     Instead, the callee should invoke
    ///     <see cref="GetCommandHelp(System.Collections.Generic.IEnumerable{Remora.Commands.Trees.Nodes.IChildNode})" />.
    /// </remarks>
    IEmbed GetCommandHelp(IChildNode command);

    /// <summary>
    ///     Creates one or more embeds for a help screen for the given child nodes.
    ///     The provided nodes may be of any type, but it's generally expected that .
    /// </summary>
    /// <param name="subCommandArray">The child commands, grouped by name.</param>
    /// <returns>One or more embeds displaying relevant information about the given commands.</returns>
    IEnumerable<IEmbed> GetCommandHelp(IEnumerable<IChildNode> subCommandArray);

    /// <summary>
    ///     Creates one or more embeds for a help screen showing the top-level commands.
    /// </summary>
    /// <param name="commands">The top-level commands of the searched tree.</param>
    /// <returns>One or more embeds displaying help for the given commands.</returns>
    IEnumerable<IEmbed> GetTopLevelHelpEmbeds(IEnumerable<IGrouping<string, IChildNode>> commands);
}