using Microsoft.AspNetCore.Mvc;
using VolleyballScraper.Api.Models;
using VolleyballScraper.Api.Services;

namespace VolleyballScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VolleyballController : ControllerBase
{
    private readonly VolleyballScraperService _scraper;
    private readonly GameCacheService _cache;

    public VolleyballController(VolleyballScraperService scraper, GameCacheService cache)
    {
        _scraper = scraper;
        _cache = cache;
    }

    [HttpGet("leagues")]
    public IActionResult GetLeagues() =>
        Ok(SupportedLeagues.All.Select(l => new
        {
            l.Code,
            l.DisplayName,
            l.Category
        }));

    [HttpPost("games")]
    public async Task<IActionResult> GetGames(
        [FromBody] FixtureRequest request,
        [FromQuery] bool forceRefresh = false)
    {
        request.OrganizationId = "662";
        request.Gender = "B";

        var games = await _scraper.GetGamesAsync(request, forceRefresh);

        return Ok(new
        {
            total = games.Count,
            season = request.SeasonId,
            cachedLeagues = _cache.GetCachedKeys()
                                  .Count(k => k.Contains(request.SeasonId)),
            leagues = games
                .GroupBy(m => m.League)
                .Select(g => new
                {
                    league = g.Key,
                    count = g.Count(),
                    games = g.ToList()
                })
        });
    }

    // Show cache status
    [HttpGet("cache/status")]
    public IActionResult GetCacheStatus() =>
        Ok(new
        {
            totalCachedKeys = _cache.GetCachedKeys().Count,
            keys = _cache.GetCachedKeys()
        });

    // Clear cache for a specific season or all
    [HttpDelete("cache")]
    public IActionResult ClearCache([FromQuery] string? seasonId = null)
    {
        _cache.Clear(seasonId);
        return Ok(new
        {
            message = seasonId == null
                ? "All cache cleared"
                : $"Cache cleared for season {seasonId}",
            seasonId
        });
    }
}