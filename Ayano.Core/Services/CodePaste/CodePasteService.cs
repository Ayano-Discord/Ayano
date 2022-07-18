using System.Drawing;
using Ayano.Core.Services.Utilities;
using Newtonsoft.Json.Linq;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.API.Abstractions.Rest;

namespace Ayano.Core.Services.CodePaste
{
    public class CodePasteService
    {
        private const string Header = @"
/*
    Written By: {0} in #{1}
    Posted on {2}
    Message ID: {3}
*/

{4}";

        private const string ApiReferenceUrl = "https://paste.mod.gg/";
        private IHttpClientFactory _httpClientFactory { get; init; }
        private IDiscordRestChannelAPI ChannelApi { get; init; }

        public CodePasteService(IHttpClientFactory httpClientFactory, IDiscordRestChannelAPI channelApi)
        {
            _httpClientFactory = httpClientFactory;
            ChannelApi = channelApi;
        }

        /// <summary>
        /// Uploads a given piece of code to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="code">The code to post</param>
        /// <returns>The URL to the newly created post</returns>
        public async Task<string> UploadCodeAsync(string code)
        {
            var content = FormatUtilities.BuildContent(code);

            var client = _httpClientFactory.CreateClient(HttpClientNames.TimeoutFiveSeconds);

            var response = await client.PostAsync($"{ApiReferenceUrl}documents", content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"{response.StatusCode} returned when calling {response.RequestMessage?.RequestUri}. Response body: {body}");
            }

            var urlResponse = await response.Content.ReadAsStringAsync();
            var pasteKey = JObject.Parse(urlResponse)["key"]?.Value<string>();

            return $"{ApiReferenceUrl}{pasteKey}";
        }

        /// <summary>
        /// Uploads the code in the given message to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="msg">The Discord message to upload</param>
        /// <param name="code">The string to upload instead of message content</param>
        /// <returns>The URL to the newly created post</returns>
        public async Task<string> UploadCodeAsync(MessageContext msg, string? code = null)
        {
            var formatted = string.Format(Header,
                $"{msg.User.Username}#{msg.User.Discriminator}", await ChannelApi.GetChannelAsync(msg.ChannelID),
                DateTimeOffset.UtcNow.ToString("dddd, MMMM d yyyy @ H:mm:ss"), msg.MessageID,
                FormatUtilities.FixIndentation(code ?? msg.Message.Content.Value));

            return await UploadCodeAsync(formatted);
        }

        public Embed BuildEmbed(IUser user, string content, string url)
        {
            var cleanCode = FormatUtilities.FixIndentation(content);

            return new Embed
                {
                    Title = "Your message was re-uploaded",
                    Description = cleanCode.Trim().Truncate(200, 6),
                    Fields = new EmbedField[] {
                        new ("Auto-Paste", url, true)
                    },
                    Colour = Color.FromArgb(1, 95, 186, 125)
                };
        }
    }
    
    public static class Extensions
    {
        public static string Truncate(this string value, int maxLength, int maxLines, string suffix = "…")
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (value.Length <= maxLength)
            {
                return value;
            }

            var lines = value.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                
            return lines.Length > maxLines ? string.Join("\n", lines.Take(maxLines)) : $"{value.Substring(0, maxLength).Trim()}{suffix}";
        }
    }

}
