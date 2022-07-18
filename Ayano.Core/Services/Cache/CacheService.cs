using System.Collections.Concurrent;
using ArgonautCore.Lw;
using Ayano.Core.Services.Cache;

namespace Ayano.Core.Services.Cache;

public partial class CacheService : ICacheService
{
    private const short _CACHE_CLEAN_DELAY = 10;

    private readonly ConcurrentDictionary<string, CacheItem> _customCache = new();

    private readonly ConcurrentDictionary<ulong, CacheItem>
        _discordCache = new();

    // I'm not sure if its a smart idea to remove all the pointers to a 
    // reoccurring event. I dont want it to be GC'd. Not sure if that is a
    // thing but i'll just leave it to be sure :)
    // ReSharper disable once NotAccessedField.Local
    private Timer _timer;

    public CacheService()
    {
        _timer = new Timer(CleanCaches, null, TimeSpan.FromSeconds(_CACHE_CLEAN_DELAY),
            TimeSpan.FromSeconds(_CACHE_CLEAN_DELAY));
    }

    public Option<T> GetOrSetAndGet<T>(ulong id, Func<T> set, TimeSpan? ttl = null)
    {
        return Option.Some(GetOrSetAndGet(id, _discordCache, set, ttl));
    }

    public async Task<Option<T>> GetOrSetAndGetAsync<T>(ulong id, Func<Task<T>> set, TimeSpan? ttl = null)
    {
        return Option.Some(await GetOrSetAndGetAsync(id, _discordCache, set, ttl).ConfigureAwait(false));
    }

    public async Task<Option<T>> TryGetOrSetAndGetAsync<T>(ulong id, Func<Task<T>> set, TimeSpan? ttl = null)
    {
        return await TryGetOrSetAndGetAsync(id, _discordCache, set, ttl).ConfigureAwait(false);
    }

    public void Set(ulong id, object obj, TimeSpan? ttl = null)
    {
        var itemToStore = new CacheItem(obj, ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null);
        _discordCache.AddOrUpdate(id, itemToStore, (key, cacheItem) => itemToStore);
    }

    public Option<T> TryRemove<T>(ulong id)
    {
        if (!_discordCache.TryRemove(id, out var cacheItem))
            return Option.None<T>();
        if (!cacheItem.IsValid()) return Option.None<T>();
        return Option.Some((T)cacheItem.Content);
    }

    public void TryRemove(ulong id)
    {
        _discordCache.TryRemove(id, out _);
    }

    public void AddOrUpdate(ulong id, CacheItem addItem, Func<ulong, CacheItem, CacheItem> updateFunc)
    {
        _discordCache.AddOrUpdate(id, addItem, updateFunc);
    }

    private async Task<Option<TReturn>> TryGetOrSetAndGetAsync<TCacheKey, TReturn>(
        TCacheKey id, ConcurrentDictionary<TCacheKey, CacheItem> cache,
        Func<Task<TReturn>> set, TimeSpan? ttl = null)
    {
        if (cache.TryGetValue(id, out var item) && item.IsValid()) return Option.Some((TReturn)item.Content);
        // Otherwise we have to set it
        var result = await set().ConfigureAwait(false);
        if (result is null) return Option.None<TReturn>();
        var itemToStore = new CacheItem(result, ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null);
        cache.AddOrUpdate(id, itemToStore, (key, cacheItem) => itemToStore);
        return Option.Some((TReturn)itemToStore.Content);
    }

    private async Task<TReturn> GetOrSetAndGetAsync<TCacheKey, TReturn>(
        TCacheKey id, ConcurrentDictionary<TCacheKey, CacheItem> cache,
        Func<Task<TReturn>> set, TimeSpan? ttl = null)
    {
        if (cache.TryGetValue(id, out var item) && item.IsValid()) return (TReturn)item.Content;
        // Otherwise we have to set it
        var result = await set().ConfigureAwait(false);
        if (result is null) throw new ArgumentException("Result of the set function was null. This is NOT acceptable");
        var itemToStore = new CacheItem(result, ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null);
        cache.AddOrUpdate(id, itemToStore, (key, cacheItem) => itemToStore);
        return (TReturn)itemToStore.Content;
    }

    private TReturn GetOrSetAndGet<TCacheKey, TReturn>(
        TCacheKey id, ConcurrentDictionary<TCacheKey, CacheItem> cache,
        Func<TReturn> set, TimeSpan? ttl = null)
    {
        if (cache.TryGetValue(id, out var item) && item.IsValid()) return (TReturn)item.Content;
        // Otherwise we have to set it
        var result = set();
        if (result is null) throw new ArgumentException("Result of the set function was null. This is NOT acceptable");
        var itemToStore = new CacheItem(result, ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null);
        cache.AddOrUpdate(id, itemToStore, (key, cacheItem) => itemToStore);
        return (TReturn)itemToStore.Content;
    }

    #region CleanCaches

    private void CleanCaches(object stateInfo)
    {
        ClearSpecificCache(_customCache);
        ClearSpecificCache(_discordCache);
    }

    private void ClearSpecificCache<T>(ConcurrentDictionary<T, CacheItem> cache)
    {
        var dKeys = cache.Keys;
        foreach (var key in dKeys)
        {
            if (!cache.TryGetValue(key, out var item)) continue;
            if (!item.IsValid()) cache.TryRemove(key, out _);
        }
    }

    #endregion

    #region Getters

    public Option<object> Get(ulong id)
    {
        if (!_discordCache.TryGetValue(id, out var item))
            return Option.None<object>();
        if (item.IsValid()) return Option.Some(item.Content);

        _discordCache.TryRemove(id, out _);
        return Option.None<object>();
    }

    public Option<T> Get<T>(ulong id)
    {
        if (!_discordCache.TryGetValue(id, out var item))
            return Option.None<T>();
        if (item.IsValid()) return Option.Some((T)item.Content);

        _discordCache.TryRemove(id, out _);
        return Option.None<T>();
    }

    public bool Contains(ulong id)
    {
        return _discordCache.ContainsKey(id);
    }

    #endregion
}