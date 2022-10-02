using System.ComponentModel;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Ayano.Plugins.Leveling.Services.ProfileService;
using Ayano.Core.Services.Cache;

namespace Ayano.Plugins.Leveling.Commands;

public class ProfileCommand : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly MessageContext _context;
    private readonly FeedbackService _feedbackService;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly HttpClientHelper _hch;

    private readonly ImageGenerator _imgGen;
    // private readonly IDiscordRestUserAPI _userApi;

    public ProfileCommand(
        ImageGenerator imgGen,
        ICacheService cacheService,
        HttpClientHelper hch,
        FeedbackService feedbackService,
        IDiscordRestChannelAPI channelApi,
        MessageContext context,
        IDiscordRestGuildAPI guildApi
    )
    {
        _imgGen = imgGen;
        _feedbackService = feedbackService;
        _channelApi = channelApi;
        _hch = hch;
        _context = context;
        //  _userApi = userApi;
        _guildApi = guildApi;
    }

    [Command("profile", "p")]
    [Category("Leveling")]
    // [Summary("Shows your or the @mentioned user's profile card with level and rank stats")]
    public async Task<Result> Command(
        //[Summary("@User or leave blank to get your own")]
        GuildMember? userT = null)
    {
        await _channelApi.TriggerTypingIndicatorAsync(_context.ChannelID);

        var user = userT ?? _guildApi.GetGuildMemberAsync(_context.GuildID.Value, _context.User.ID).Result.Entity;


        // var userStatsM = await _profileRepo.GetProfileStatistics(user.Id, Context.Guild.Id).ConfigureAwait(false);
        // if (!userStatsM.HasValue)
        // {
        //     await ReplyFailureEmbed(
        //         $"{Formatter.UsernameDiscrim(user)} is not in my Database :/ Make sure he used or chatted with Sora at least once.");
        //     return;
        // }

        try
        {
            // First get their avatar

            await _hch.DownloadAndSaveFile(
                    CDN.GetUserAvatarUrl(user.User.Value).Entity,
                    Path.Combine(_imgGen.ImageGenPath, ImageGenerator.AVATAR_CACHE, $"{user.User.Value.ID}.png"))
                .ConfigureAwait(false);

            // Now generate image
            var filePath = Path.Combine(_imgGen.ImageGenPath, ImageGenerator.PROFILE_CARDS,
                $"{user.User.Value.ID}.png");

            _imgGen.GenerateProfileImage(_context, new ProfileImageGenDto
            {
                UserId = user.User.Value.ID,
                Name = user.User.Value.Username,
                GlobalExp = 1000,
                GlobalLevel = 10,
                GlobalRank = 1,
                GlobalNextLevelExp = 20,
                HasCustomBg = true,
                LocalExp = 100,
                LocalRank = 1,
                LocalLevel = 1,
                LocalNextLevelExp = 10,
                ClanName = "Test",
                DefaultBg = true,
                BgPath = "path:defaultBG2"
            }, filePath);

            await _channelApi.CreateMessageAsync(
                _context.ChannelID,
                attachments: new[]
                {
                    OneOf<FileData, IPartialAttachment>.FromT0(
                        new FileData($"{user.User.Value.ID}.png", new FileStream(filePath, FileMode.Open)))
                }
            );
            // We no longer need the image
            File.Delete(Path.Combine(_imgGen.ImageGenPath, "ProfileBackgrounds", $"{user.User.Value.ID.Value}.png"));
        }
        catch (Exception e)
        {
            await _feedbackService.SendContextualErrorAsync(
                "Failed to generate image. Something went wrong sorry :/" +
                "This could have multiple reasons. One of them could be that your username has characters that " +
                "are currently not supported. This is any weird character that you wouldn't naturally find on your standard " +
                "keyboard.");
            Console.WriteLine(e);
            if (e.InnerException is NotImplementedException)
                return Result.FromError<string>(e.InnerException.Message);
        }
        finally
        {
            // Remove avatar
            var avatar = Path.Combine(_imgGen.ImageGenPath, ImageGenerator.AVATAR_CACHE,
                $"{user.User.Value.ID}.png");
            if (File.Exists(avatar))
                File.Delete(avatar);

            // Remove profile image
            var profileImg = Path.Combine(_imgGen.ImageGenPath, ImageGenerator.PROFILE_CARDS,
                $"{user.User.Value.ID}.png");
            if (File.Exists(profileImg))
                File.Delete(profileImg);
        }

        return Result.FromSuccess();
    }
}