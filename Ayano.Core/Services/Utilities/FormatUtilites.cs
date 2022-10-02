namespace Ayano.Core.Services.Utilities;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Remora.Discord;

using Humanizer;
using Humanizer.Localisation;

using Ayano.Core.Services.CodePaste;

public static class FormatUtilities
{
    private static readonly Regex _buildContentRegex = new Regex(@"```([^\s]+|)");

    /// <summary>
    /// Prepares a piece of input code for use in HTTP operations
    /// </summary>
    /// <param name="code">The code to prepare</param>
    /// <returns>The resulting StringContent for HTTP operations</returns>
    public static StringContent BuildContent(string code)
    {
        var cleanCode = StripFormatting(code);
        return new StringContent(cleanCode, Encoding.UTF8, "text/plain");
    }

    public static string StripFormatting(string code)
    {
        var cleanCode = _buildContentRegex.Replace(code.Trim(), string.Empty); //strip out the ` characters and code block markers
        cleanCode = cleanCode.Replace("\t", "    "); //spaces > tabs
        cleanCode = FixIndentation(cleanCode);
        return cleanCode;
    }

    /// <summary>
    /// Attempts to fix the indentation of a piece of code by aligning the left sidie.
    /// </summary>
    /// <param name="code">The code to align</param>
    /// <returns>The newly aligned code</returns>
    public static string FixIndentation(string code)
    {
        var lines = code.Split('\n');
        var indentLine = lines.SkipWhile(d => d.FirstOrDefault() != ' ').FirstOrDefault();

        if (indentLine != null)
        {
            var indent = indentLine.LastIndexOf(' ') + 1;

            var pattern = $@"^[^\S\n]{{{indent}}}";

            return Regex.Replace(code, pattern, "", RegexOptions.Multiline);
        }

        return code;
    }

    // public static async Task UploadToServiceIfBiggerThan(this EmbedBuilder embed, string content, uint size, CodePasteService service)
    // {
    //     if (content.Length > size)
    //     {
    //         try
    //         {
    //             var resultLink = await service.UploadCodeAsync(content);
    //             embed.AddField(a => a.WithName("More...").WithValue($"[View on Hastebin]({resultLink})"));
    //         }
    //         catch (WebException we)
    //         {
    //             embed.AddField(a => a.WithName("More...").WithValue(we.Message));
    //         }
    //     }
    // }

    public static string SanitizeAllMentions(string text)
    {
        var everyoneSanitized = SanitizeEveryone(text);
        var userSanitized = SanitizeUserMentions(everyoneSanitized);
        var roleSanitized = SanitizeRoleMentions(userSanitized);

        return roleSanitized;
    }

    public static string SanitizeEveryone(string text)
        => text.Replace("@everyone", "@\x200beveryone")
               .Replace("@here", "@\x200bhere");

    public static string SanitizeUserMentions(string text)
        => _userMentionRegex.Replace(text, "<@\x200b${Id}>");

    public static string SanitizeRoleMentions(string text)
        => _roleMentionRegex.Replace(text, "<@&\x200b${Id}>");

    private static readonly Regex _userMentionRegex = new Regex("<@!?(?<Id>[0-9]+)>", RegexOptions.Compiled);

    private static readonly Regex _roleMentionRegex = new Regex("<@&(?<Id>[0-9]+)>", RegexOptions.Compiled);

    public static string FormatTimeAgo(DateTimeOffset now, DateTimeOffset ago)
    {
        var span = now - ago;

        var humanizedTimeAgo = span > TimeSpan.FromSeconds(60)
            ? span.Humanize(maxUnit: TimeUnit.Year, culture: CultureInfo.InvariantCulture)
            : "a few seconds";

        return $"{humanizedTimeAgo} ago ({ago.UtcDateTime:yyyy-MM-ddTHH:mm:ssK})";
    }

    public static bool ContainsSpoiler(string text)
        => _containsSpoilerRegex.IsMatch(text);

    private static readonly Regex _containsSpoilerRegex
        = new Regex(@"\|\|.+\|\|", RegexOptions.Compiled);

#nullable enable
    public static string FormatCodeForEmbed(string language, string sourceCode, int maxLength)
    {
        if (maxLength <= 0)
            return string.Empty;

        // Trim, for good measure
        sourceCode = sourceCode.Trim();
        var processedLines = new List<string>();
        var braceOnlyLinesEliminated = 0;
        var currentLength = 0;

        var lines = sourceCode
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.TrimEnd())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToImmutableArray();

        // Embeds always end up being too long and then messy to display in chat. They also disrupt conversations
        // a lot as they just end up being a huge wall of text out of nowhere. To account for this, we will:
        //   - Trim all lines at the right side
        //   - Remove blank lines
        //   - Change all single-line opening brackets to be on the previous line instead
        //   - Clip every line to the maximum length in an embed
        //   - Clip the total number of lines to 10
        //   - If there's more, add a comment indicating the number of remaining lines
        foreach (var line in lines)
        {
            const int MaxLineLength = 61;

            if (line.Length < MaxLineLength - 1)
            {
                if (line.EndsWith('{') &&
                    string.IsNullOrWhiteSpace(line[..^1]) &&
                    processedLines.Count > 0)
                {
                    if (!TryReplaceLine(^1, processedLines[^1] + " {"))
                    {
                        AddRemainingLineComment();
                        break;
                    }
                    braceOnlyLinesEliminated++;
                }
                else if (!TryAddLine(line))
                {
                    AddRemainingLineComment();
                    break;
                }
            }
            else
            {
                if (!TryAddLine(line[..(MaxLineLength - 3)] + "..."))
                {
                    AddRemainingLineComment();
                    break;
                }
            }

            if (processedLines.Count == 10)
            {
                var remainingCount = GetRemainingLineCount();

                // Might as well just add the last line to the embed,
                // since we'd just be adding a "1 more line" line otherwise.
                // We'd be adding a line either way, so we should go with the
                // more useful option.
                if (remainingCount == 1)
                    continue;

                if (remainingCount <= 0)
                    break;

                AddRemainingLineComment();
                break;
            }
        }

        var code = string.Join('\n', processedLines);
        return $"```{language}\n{code}\n```";

        bool TryAddLine(string line)
        {
            var remainingCount = GetRemainingLineCount();
            var possibleRemainingLineCommentLength = remainingCount > 1 // 1, because the current line is included in the count
                ? GetRemainingLineCountComment(remainingCount).Length
                : 0;

            if (line.Length + currentLength + possibleRemainingLineCommentLength + 1 > maxLength) // +1 because of the newline that will be added later
                return false;

            processedLines.Add(line);
            currentLength += line.Length + 1; // +1 because of the newline that will be added later
            return true;
        }

        bool TryReplaceLine(Index index, string line)
        {
            var remainingCount = GetRemainingLineCount();
            var possibleRemainingLineCommentLength = remainingCount > 0
                ? GetRemainingLineCountComment(remainingCount).Length
                : 0;

            var lengthDifference = line.Length - processedLines[index].Length;

            if (lengthDifference + currentLength + possibleRemainingLineCommentLength > maxLength)
                return false;

            processedLines[index] = line;
            currentLength += lengthDifference;
            return true;
        }

        int GetRemainingLineCount()
            => lines.Length - processedLines.Count - braceOnlyLinesEliminated;

        string GetRemainingLineCountComment(int remainingCount)
        {
            var commentStart = language switch
            {
                "vb" => "'",
                _ => "//",
            };

            return $"{commentStart} {remainingCount} more lines. Follow the link to view.";
        }

        void AddRemainingLineComment()
        {
            processedLines.Add(GetRemainingLineCountComment(GetRemainingLineCount()));
        }
    }
#nullable restore
}
