namespace VolleyballScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VolleyballController : ControllerBase
{
    private readonly VolleyballScraperService _scraper;

    public VolleyballController(VolleyballScraperService scraper)
        => _scraper = scraper;

    // List supported leagues
    [HttpGet("leagues")]
    public IActionResult GetLeagues() =>
        Ok(SupportedLeagues.All.Select(l => new
        {
            l.Code,
            l.DisplayName,
            l.Category
        }));

    // Get games
    [HttpPost("games")]
    public async Task<IActionResult> GetGames([FromBody] FixtureRequest request)
    {
        // KurumId and Gender are fixed — cannot be overridden from outside
        request.OrganizationId = "662";
        request.Gender = "B";

        var games = await _scraper.GetGamesAsync(request);

        return Ok(new
        {
            total = games.Count,
            season = request.SeasonId,
            leagues = games.GroupBy(m => m.League)
                           .Select(g => new
                           {
                               league = g.Key,
                               count = g.Count(),
                               games = g.ToList()
                           })
        });
    }
}