namespace OlympicScraper.Api.Services.Volleyball;

public interface IStandingsCacheService
{
    string BuildCompetitionsKey(string seasonId, string category, string leagueCode);
    string BuildStandingsKey(string seasonId, string competitionName);
    bool TryGetCompetitions(string key, out List<Competition> competitions);
    bool TryGetStandings(string key, out Response standings);
    void SetCompetitions(string key, List<Competition> competitions);
    void SetStandings(string key, Response standings);
    void Clear();
    void Clear(string seasonId);
    void ClearCache();
    List<string> GetCachedKeys();
}