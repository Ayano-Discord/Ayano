namespace Ayano.Core.Services.Cache;

public class CacheItem
{
    public CacheItem(object content, DateTime? validUntil)
    {
        Content = content;
        ValidUntil = validUntil;
    }

    public CacheItem(object content, in TimeSpan timeSpan)
    {
        Content = content;
        ValidUntil = DateTime.UtcNow.Add(timeSpan);
    }

    public object Content { get; }
    public DateTime? ValidUntil { get; }

    public bool IsValid()
    {
        if (ValidUntil.HasValue && ValidUntil.Value.CompareTo(DateTime.UtcNow) <= 0) return false;

        return true;
    }
}