namespace OlympicScraper.Api.Services.Volleyball;

public interface IFixtureCacheService
{
    string BuildKey(string seasonId, string leagueCode);
    bool TryGet(string key, out List<Game> games);
    void Set(string key, List<Game> games);
    void Remove(string key);
    void Clear(string? seasonId = null);
    List<string> GetCachedKeys();
}