using System.ComponentModel;
using Ayano.Core.Services.Cache;
using Ayano.Plugins.Leveling.Services.ProfileService;
using Ayano.Core.Services.Cache.Services;
using Core.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Ayano.Plugins.Leveling.Commands;

[Group("profileset")]
public class ProfileSet : CommandGroup
{
    private const int _SET_BG_COOLDOWN_S = 45;
    private readonly ICacheService _cacheService;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly MessageContext _context;
    private readonly HttpClientHelper _hch;
    private readonly ImageGenerator _imgGen;

    public ProfileSet(
        ImageGenerator imgGen,
        ICacheService cacheService,
        HttpClientHelper hch,
        IDiscordRestChannelAPI channelApi,
        MessageContext messageContext
    )
    {
        _imgGen = imgGen;
        _cacheService = cacheService;
        _hch = hch;
        _context = messageContext;
        _channelApi = channelApi;
    }

    [Command("background", "bg")]
    [Description("Sets your profile background to the given image")]
    public async Task<IResult> SetBackground(string url = "")
    {
        // Check cooldown
        var cd = _cacheService.Get<DateTime>(CacheId.BgCooldownId(_context.User.ID.Value));
        if (cd.HasValue)
        {
            var secondsRemaining = cd.Some().Subtract(DateTime.UtcNow.TimeOfDay).Second;
            return await _channelApi.CreateMessageAsync(_context.ChannelID,
                $"Dont break me >.< Please wait another {secondsRemaining.ToString()} seconds!");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            if (_context.Message.Attachments.Value.Count != 1)
                return await _channelApi.CreateMessageAsync(
                    _context.ChannelID,
                    "If you do not provide an image link or a number from 1-4, then you MUST provide 1 Attached image");

            url = _context.Message.Attachments.Value[0].Url;


            // Check if attachment is a valid image
            if (!Helper.LinkIsImage(url))
                return await _channelApi.CreateMessageAsync(_context.ChannelID,
                    "The provided attachment is not an image! " +
                    "Make sure the attachment ends with any of these extensions: `.jpg, .png, .gif, .jpeg`");

            _cacheService.Set(CacheId.ProfileBackgroundId(_context.User.ID.Value), url);
            _cacheService.Set(CacheId.BgCooldownId(_context.User.ID.Value),
                DateTime.UtcNow.AddSeconds(_SET_BG_COOLDOWN_S),
                TimeSpan.FromSeconds(_SET_BG_COOLDOWN_S));

            return await _channelApi.CreateMessageAsync(_context.ChannelID,
                "Successfully updated your profile card background!");

        }


        try
        {
            int.TryParse(url, out var num);

            if (!((num >= 0) & (num <= 5)))
                return await _channelApi.CreateMessageAsync(_context.ChannelID,
                    "You must provide a number from 1-4");

            _cacheService.Set(CacheId.ProfileBackgroundId(_context.User.ID.Value), url);
            _cacheService.Set(CacheId.BgCooldownId(_context.User.ID.Value),
                DateTime.UtcNow.AddSeconds(_SET_BG_COOLDOWN_S),
                TimeSpan.FromSeconds(_SET_BG_COOLDOWN_S));

            return await _channelApi.CreateMessageAsync(_context.ChannelID,
                "Successfully updated your profile card background!");
        }
        catch (FormatException)
        {
            // Check if the URL is a valid image
            if (!Helper.LinkIsImage(url))
                return await _channelApi.CreateMessageAsync(_context.ChannelID,
                    "The provided link is not an image! " +
                    "Make sure the link ends with any of these extensions: `.jpg, .png, .gif, .jpeg`");

            _cacheService.Set(CacheId.ProfileBackgroundId(_context.User.ID.Value), url);

            // Set BG on user
            // Add cooldown
            _cacheService.Set(CacheId.BgCooldownId(_context.User.ID.Value),
                DateTime.UtcNow.AddSeconds(_SET_BG_COOLDOWN_S),
                TimeSpan.FromSeconds(_SET_BG_COOLDOWN_S));

            return await _channelApi.CreateMessageAsync(_context.ChannelID,
                "Successfully updated your profile card background!");
        }
    }
}