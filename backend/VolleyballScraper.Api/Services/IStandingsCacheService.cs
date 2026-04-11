using VolleyballScraper.Api.Models.Standings;

namespace VolleyballScraper.Api.Services;

public interface IStandingsCacheService
{
    string BuildCompetitionsKey(string seasonId, string category, string leagueCode);
    string BuildStandingsKey(string seasonId, string competitionName);
    bool TryGetCompetitions(string key, out List<Competition> competitions);
    bool TryGetStandings(string key, out StandingsResponse standings);
    void SetCompetitions(string key, List<Competition> competitions);
    void SetStandings(string key, StandingsResponse standings);
    void Clear(string? seasonId = null);
    void ClearCache();
    List<string> GetCachedKeysng? seasonId = null);
    void ClearCache();
    List<string> GetCachedKeys();
}