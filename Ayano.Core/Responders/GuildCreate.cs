using Ayano.Core.Services.Cache;
using Ayano.Core.Utilities;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Ayano.Core.Responders;

public class GuildCreateResponder: IResponder<IGuildCreate>
{    
    private IDiscordRestChannelAPI DiscordRestChannelApi { get; init; }
    private IDiscordRestUserAPI DiscordRestUserApi { get; init; }
    private ICacheService CacheService { get; init; }

    public GuildCreateResponder(
        IDiscordRestUserAPI discordRestUserApi,
        IDiscordRestChannelAPI discordRestChannelApi,
        ICacheService cacheService
        )
    {
        DiscordRestUserApi = discordRestUserApi;
        DiscordRestChannelApi = discordRestChannelApi;
        CacheService = cacheService;
    }
    

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        var guildCount = CacheService.Get<int>("global:guildCount").Some();
        
        CacheService.Set("global:guildCount", guildCount + 1);
        
        var channelCount = CacheService.Get<int>("global:channel:totalChannelCount").Some();
        
        CacheService.Set("global:channel:totalChannelCount", channelCount + gatewayEvent.Channels.Count);

        var userCount = CacheService.Get<int>("global:userCount").Some();
        
        CacheService.Set("global:userCount", userCount + gatewayEvent.MemberCount);
        
        AyanoMetrics.GatewayEventReceieved.WithLabels("guildCreate").Inc();
        
        return Result.FromSuccess();
    }
}