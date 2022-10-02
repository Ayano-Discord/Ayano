namespace Ayano.Core.Services.Cache.Services;

public static class CacheId
{
    public const string WAIFU_RARITY_STATISTICS = "wrs";

    public static ulong PrefixCacheId(ulong guildId)
    {
        return guildId;
    }

    public static string BgCooldownId(ulong userId)
    {
        return "setbg:" + userId;
    }

    public static string ProfileBackgroundId(ulong userId)
    {
        return "bg:" + userId;
    }

    public static string StarboardDoNotPostId(ulong messageId)
    {
        return "star:" + messageId;
    }

    public static string StarboardUserMessageReactCountId(ulong messageId, ulong userId)
    {
        return messageId + userId.ToString();
    }

    public static string GetGuildUser(ulong userId, ulong guildId)
    {
        return userId + guildId.ToString();
    }

    public static ulong GetUser(ulong userId)
    {
        return userId;
    }

    public static ulong GetMessageId(ulong messageId)
    {
        return messageId;
    }

    public static string MusicCacheMessage(ulong guildId)
    {
        return $"ms:{guildId.ToString()}";
    }

    public static string GetAfkId(ulong userId)
    {
        return $"afk:{userId.ToString()}";
    }

    public static string GetAfkCheckId(ulong userId)
    {
        return $"afk:{userId.ToString()}:c";
    }
}