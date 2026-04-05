using Microsoft.AspNetCore.Mvc;
using VolleyballScraper.Api.Models;
using VolleyballScraper.Api.Services;

namespace VolleyballScraper.Api.Controllers;

/// <summary>
/// Endpoints for fetching Göztepe volleyball fixture data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VolleyballController : ControllerBase
{
    private readonly VolleyballScraperService _scraper;
    private readonly GameCacheService _cache;

    public VolleyballController(VolleyballScraperService scraper, GameCacheService cache)
    {
        _scraper = scraper;
        _cache = cache;
    }

    /// <summary>
    /// Returns all supported leagues.
    /// </summary>
    /// <returns>List of league codes, display names and categories.</returns>
    [HttpGet("leagues")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLeagues() =>
        Ok(SupportedLeagues.All.Select(l => new
        {
            l.Code,
            l.DisplayName,
            l.Category
        }));

    /// <summary>
    /// Fetches fixture data for Göztepe from the specified season and leagues.
    /// </summary>
    /// <param name="request">Season ID and optional league filter.</param>
    /// <param name="forceRefresh">If true, bypasses cache and re-fetches from site.</param>
    /// <returns>Grouped fixture data by league.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/volleyball/games
    ///     {
    ///         "seasonId": "2025-2026",
    ///         "leagues": ["GKSL", "YKSL"]
    ///     }
    ///
    /// Leave `leagues` empty to fetch all supported leagues.
    /// OrganizationId and Gender are fixed server-side (Göztepe / Women).
    /// </remarks>
    [HttpPost("games")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
            cachedLeagues = _cache.GetCachedKeys().Count(k => k.Contains(request.SeasonId)),
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

    /// <summary>
    /// Returns the current cache status — which league/season combinations are cached.
    /// </summary>
    [HttpGet("cache/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCacheStatus() =>
        Ok(new
        {
            totalCachedKeys = _cache.GetCachedKeys().Count,
            keys = _cache.GetCachedKeys()
        });

    /// <summary>
    /// Clears cached fixture data.
    /// </summary>
    /// <param name="seasonId">
    /// If provided, clears only that season (e.g. "2025-2026").
    /// If omitted, clears all cached data.
    /// </param>
    [HttpDelete("cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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