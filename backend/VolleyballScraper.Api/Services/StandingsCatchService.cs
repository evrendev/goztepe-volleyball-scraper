using Microsoft.Extensions.Caching.Memory;

namespace VolleyballScraper.Api.Services;

public class StandingsCacheService : IStandingsCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<StandingsCacheService> _logger;

    // Standings and competition data changes infrequently — cache for 30 days
    private static readonly TimeSpan CacheDuration =
        TimeSpan.FromHours(AppConstants.StandingsCacheDuration);

    private readonly HashSet<string> _trackedKeys = [];
    private readonly Lock _keyLock = new();

    public StandingsCacheService(IMemoryCache cache, ILogger<StandingsCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    // Key format: "competitions:{seasonId}:{category}:{leagueCode}"
    public string BuildCompetitionsKey(string seasonId, string category, string leagueCode) =>
        $"competitions:{seasonId}:{category}:{leagueCode}";

    // Key format: "standings:{seasonId}:{competitionId}"
    public string BuildStandingsKey(string seasonId, string competitionName) =>
        $"standings:{seasonId}:{competitionName}";

    public bool TryGetCompetitions(string key, out List<Competition> competitions)
    {
        if (_cache.TryGetValue(key, out List<Competition>? cached) && cached != null)
        {
            _logger.LogInformation("Standings cache HIT: {key}", key);
            competitions = cached;
            return true;
        }

        _logger.LogInformation("Standings cache MISS: {key}", key);
        competitions = [];
        return false;
    }

    public bool TryGetStandings(string key, out StandingsResponse standings)
    {
        if (_cache.TryGetValue(key, out StandingsResponse? cached) && cached != null)
        {
            _logger.LogInformation("Standings cache HIT: {key}", key);
            standings = cached;
            return true;
        }

        _logger.LogInformation("Standings cache MISS: {key}", key);
        standings = new StandingsResponse();
        return false;
    }

    public void SetCompetitions(string key, List<Competition> competitions)
    {
        _cache.Set(key, competitions, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            Priority = CacheItemPriority.Normal,
        });

        lock (_keyLock) _trackedKeys.Add(key);

        _logger.LogInformation(
            "Standings cache SET: {key} → {count} competitions, {dur}h valid",
            key, competitions.Count, CacheDuration.TotalHours);
    }

    public void SetStandings(string key, StandingsResponse standings)
    {
        _cache.Set(key, standings, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            Priority = CacheItemPriority.Normal,
        });

        lock (_keyLock) _trackedKeys.Add(key);

        _logger.LogInformation(
            "Standings cache SET: {key} → {teams} teams, {games} games, {dur}h valid",
            key, standings.Standings.Count, standings.Games.Count, CacheDuration.TotalHours);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        lock (_keyLock) _trackedKeys.Remove(key);
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
            lock (_keyLock) _trackedKeys.Remove(key);
        }

        _logger.LogInformation("Standings cache cleared: {count} keys removed", toRemove.Count);
    }

    public void ClearCache() => Clear();

    public List<string> GetCachedKeys()
    {
        lock (_keyLock) return [.. _trackedKeys];
    }
}