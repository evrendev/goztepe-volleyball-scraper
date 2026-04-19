using OlympicScraper.Api.Models.Standings;

namespace OlympicScraper.Api.Services;

public interface IStandingsScraperService
{
    Task<List<Competition>> GetCompetitionsAsync(CompetitionRequest request);
    Task<StandingsResponse> GetStandingsAsync(StandingsRequest request);
}