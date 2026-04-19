namespace OlympicScraper.Api.Services.Volleyball;

public interface IStandingsScraperService
{
    Task<List<Competition>> GetCompetitionsAsync(CompetitionRequest request);
    Task<Response> GetStandingsAsync(StandingsRequest request);
}