namespace OlympicScraper.Api.Services.Volleyball;

public interface IFixtureScraperService
{
    Task<string> GetRawAsync(string seasonId, string leagueCode);
    Task<List<Game>> GetGamesAsync(FixtureRequest request, bool forceRefresh = false);
}