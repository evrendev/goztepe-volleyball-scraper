namespace VolleyballScraper.Api.Services;

public class MatchCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MatchCacheService> _logger;

    // Cache duration — fixtures rarely change
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    // Track which keys are in cache (for cleanup)
    private readonly HashSet<string> _trackedKeys = [];
    private readonly Lock _keyLock = new();

    public MatchCacheService(IMemoryCache cache, ILogger<MatchCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public string BuildKey(string seasonId, string leagueCode) =>
        $"games:{seasonId}:{leagueCode}";

    public bool TryGet(string key, out List<Match> matches)
    {
        if (_cache.TryGetValue(key, out List<Match>? cached) && cached != null)
        {
            _logger.LogInformation("Cache HIT: {key}", key);
            matches = cached;
            return true;
        }

        _logger.LogInformation("Cache MISS: {key}", key);
        matches = [];
        return false;
    }

    public void Set(string key, List<Match> matches)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = null,
            Priority = CacheItemPriority.Normal,
        };

        _cache.Set(key, matches, options);

        lock (_keyLock)
            _trackedKeys.Add(key);

        _logger.LogInformation("Cache SET: {key} → {count} matches, {dur} hours valid",
            key, matches.Count, CacheDuration.TotalHours);
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