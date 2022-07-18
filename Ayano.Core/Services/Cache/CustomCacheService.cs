using ArgonautCore.Lw;

namespace Ayano.Core.Services.Cache;

public partial class CacheService
{
    public Option<object> Get(string id)
    {
        if (!_customCache.TryGetValue(id, out var item))
            return Option.None<object>();
        if (item.IsValid()) return Option.Some(item.Content);

        _customCache.TryRemove(id, out _);
        return Option.None<object>();
    }

    public Option<T> Get<T>(string id)
    {
        if (!_customCache.TryGetValue(id, out var item))
            return Option.None<T>();
        if (item.IsValid()) return Option.Some((T)item.Content);

        _customCache.TryRemove(id, out _);
        return Option.None<T>();
    }

    public bool Contains(string id)
    {
        return _customCache.ContainsKey(id);
    }

    public Option<T> GetOrSetAndGet<T>(string id, Func<T> set, TimeSpan? ttl = null)
    {
        return Option.Some(GetOrSetAndGet(id, _customCache, set, ttl));
    }

    public async Task<Option<T>> GetOrSetAndGetAsync<T>(string id, Func<Task<T>> set, TimeSpan? ttl = null)
    {
        return Option.Some(await GetOrSetAndGetAsync(id, _customCache, set, ttl).ConfigureAwait(false));
    }

    public async Task<Option<T>> TryGetOrSetAndGetAsync<T>(string id, Func<Task<T>> set, TimeSpan? ttl = null)
    {
        return await TryGetOrSetAndGetAsync(id, _customCache, set, ttl).ConfigureAwait(false);
    }

    public void Set(string id, object obj, TimeSpan? ttl = null)
    {
        var itemToStore = new CacheItem(obj, ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null);
        _customCache.AddOrUpdate(id, itemToStore, (key, cacheItem) => itemToStore);
    }

    public void AddOrUpdate(string id, CacheItem addItem, Func<string, CacheItem, CacheItem> updateFunc)
    {
        _customCache.AddOrUpdate(id, addItem, updateFunc);
    }

    public Option<T> TryRemove<T>(string id)
    {
        if (!_customCache.TryRemove(id, out var cacheItem))
            return Option.None<T>();
        if (!cacheItem.IsValid()) return Option.None<T>();
        return Option.Some((T)cacheItem.Content);
    }

    public void TryRemove(string id)
    {
        _customCache.TryRemove(id, out _);
    }
}