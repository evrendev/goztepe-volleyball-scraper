using OlympicScraper.Api.Models.Standings;

namespace OlympicScraper.Api.Services;

public interface IStandingsCacheService
{
    string BuildCompetitionsKey(string seasonId, string category, string leagueCode);
    string BuildStandingsKey(string seasonId, string competitionName);
    bool TryGetCompetitions(string key, out List<Competition> competitions);
    bool TryGetStandings(string key, out StandingsResponse standings);
    void SetCompetitions(string key, List<Competition> competitions);
    void SetStandings(string key, StandingsResponse standings);
    void Clear();
    void Clear(string seasonId);
    void ClearCache();
    List<string> GetCachedKeys();
}