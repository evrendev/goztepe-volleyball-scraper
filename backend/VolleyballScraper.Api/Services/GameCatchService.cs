using Microsoft.Extensions.Caching.Memory;
using VolleyballScraper.Api.Constants;
using VolleyballScraper.Api.Models;

namespace VolleyballScraper.Api.Services;

public class GameCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<GameCacheService> _logger;

    // Cache duration — fixtures rarely change
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(AppConstants.CacheDuration);

    // Track which keys are in cache (for cleanup)
    private readonly HashSet<string> _trackedKeys = [];
    private readonly Lock _keyLock = new();

    public GameCacheService(ILogger<GameCacheService> logger,
        IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public string BuildKey(string seasonId, string leagueCode) =>
        $"games:{seasonId}:{leagueCode}";

    public bool TryGet(string key, out List<Game> games)
    {
        if (_cache.TryGetValue(key, out List<Game>? cached) && cached != null)
        {
            _logger.LogInformation("Cache HIT: {key}", key);
            games = cached;
            return true;
        }

        _logger.LogInformation("Cache MISS: {key}", key);
        games = [];
        return false;
    }

    public void Set(string key, List<Game> games)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = null,
            Priority = CacheItemPriority.Normal,
        };

        _cache.Set(key, games, options);

        lock (_keyLock)
            _trackedKeys.Add(key);

        _logger.LogInformation("Cache SET: {key} → {count} matches, {dur} hours valid",
            key, games.Count, CacheDuration.TotalHours);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        lock (_keyLock)
            _trackedKeys.Remove(key);
    }

    public void Clear(string? seasonId = null)
    {
        List<string> toRemove;

        lock (_keyLock)
        {
            toRemove = seasonId == null
                ? [.. _trackedKeys]
                : _trackedKeys.Where(k => k.Contains($":{seasonId}:")).ToList();
        }

        foreach (var key in toRemove)
        {
            _cache.Remove(key);
            lock (_keyLock)
                _trackedKeys.Remove(key);
        }

        _logger.LogInformation("Cache cleared: {count} keys removed", toRemove.Count);
    }

    public List<string> GetCachedKeys()
    {
        lock (_keyLock)
            return [.. _trackedKeys];
    }
}