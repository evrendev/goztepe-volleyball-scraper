using VolleyballScraper.Api.Models.Standings;

namespace VolleyballScraper.Api.Services;

public interface IStandingsScraperService
{
    Task<List<Competition>> GetCompetitionsAsync(CompetitionRequest request);
    Task<StandingsResponse> GetStandingsAsync(StandingsRequest request);
}