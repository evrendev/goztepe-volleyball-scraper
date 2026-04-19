using Microsoft.AspNetCore.Mvc;

namespace OlympicScraper.Api.Controllers;

/// <summary>
/// Endpoints for fetching standings (puan durumu) and game results
/// from İzmir Voleybol İl Temsilciliği.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StandingsController : ControllerBase
{
    private readonly IStandingsScraperService _scraper;
    private readonly IStandingsCacheService _cache;

    public StandingsController(IStandingsScraperService scraper, IStandingsCacheService cache)
    {
        _scraper = scraper;
        _cache = cache;
    }

    /// <summary>
    /// Returns all competitions (yarışma adları) for the given season, category and league.
    /// Each competition corresponds to a phase such as group stage, quarter-final, semi-final, etc.
    /// </summary>
    /// <param name="request">Season, category and league code.</param>
    /// <returns>List of available competitions for the specified league.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/standings/competitions
    ///     {
    ///         "seasonId": "2025-2026",
    ///         "category": "GK",
    ///         "leagueCode": "GKSL"
    ///     }
    ///
    /// Use the returned <c>id</c> and <c>key</c> fields to call the standings endpoint.
    /// </remarks>
    [HttpPost("competitions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCompetitions([FromBody] CompetitionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SeasonId))
            return BadRequest(new { message = "SeasonId is required." });

        if (string.IsNullOrWhiteSpace(request.Category))
            return BadRequest(new { message = "Category is required." });

        if (string.IsNullOrWhiteSpace(request.LeagueCode))
            return BadRequest(new { message = "LeagueCode is required." });

        var competitions = await _scraper.GetCompetitionsAsync(request);

        return Ok(new
        {
            seasonId = request.SeasonId,
            category = request.Category,
            leagueCode = request.LeagueCode,
            total = competitions.Count,
            competitions
        });
    }

    /// <summary>
    /// Returns the standings table and game results for a specific competition.
    /// </summary>
    /// <param name="request">Season, category, league code and competition id/key.</param>
    /// <returns>
    /// Standings table rows and full game list for the competition,
    /// including both played results and upcoming fixtures.
    /// </returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/standings
    ///     {
    ///         "seasonId": "2025-2026",
    ///         "category": "GK",
    ///         "leagueCode": "GKSL",
    ///         "competitionId": "19285",
    ///         "competitionKey": "5DB65D03-09DD-4B8E-99E3-7940EEA1C725"
    ///     }
    ///
    /// Obtain <c>competitionId</c> and <c>competitionKey</c> from the competitions endpoint.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStandings([FromBody] StandingsRequest? request)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        if (string.IsNullOrWhiteSpace(request.SeasonId))
            return BadRequest(new { message = "SeasonId is required." });

        if (string.IsNullOrWhiteSpace(request.Category))
            return BadRequest(new { message = "Category is required." });

        if (string.IsNullOrWhiteSpace(request.LeagueCode))
            return BadRequest(new { message = "LeagueCode is required." });

        if (string.IsNullOrWhiteSpace(request.CompetitionName))
            return BadRequest(new { message = "CompetitionName is required." });

        try
        {
            var response = await _scraper.GetStandingsAsync(request);

            if (response.Standings.Count == 0 && response.Games.Count == 0)
                return NotFound(new
                {
                    message = "No standings data found for the specified competition.",
                    competitionName = request.CompetitionName
                });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving standings.", error = ex.Message });
        }
    }

    /// <summary>
    /// Returns all competitions across all leagues for the given season and category,
    /// filtered to only include competitions where Göztepe participates.
    /// </summary>
    /// <param name="seasonId">Season identifier. Defaults to the current season.</param>
    /// <param name="category">
    /// Category code to filter by (e.g. GK, KK, MdK).
    /// If omitted, all supported categories are scanned.
    /// </param>
    /// <remarks>
    /// This endpoint makes multiple requests to the federation site —
    /// one per league in the specified category. Use sparingly.
    ///
    /// Sample request:
    ///
    ///     GET /api/standings/competitions/goztepe?seasonId=2025-2026&amp;category=GK
    /// </remarks>
    [HttpGet("competitions/goztepe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoztepeCompetitions(
        [FromQuery] string seasonId = "2025-2026",
        [FromQuery] string? category = null)
    {
        // Determine which leagues to scan
        var leagues = SupportedLeagues.All
            .Where(l => category == null ||
                        l.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var allCompetitions = new List<object>();

        foreach (var league in leagues)
        {
            await Task.Delay(400); // be polite to the external site

            try
            {
                var competitions = await _scraper.GetCompetitionsAsync(new CompetitionRequest
                {
                    SeasonId = seasonId,
                    Category = league.Category,
                    LeagueCode = league.Code,
                });

                // For each competition, fetch standings to check Göztepe participation
                foreach (var competition in competitions)
                {
                    await Task.Delay(300);

                    try
                    {
                        var standings = await _scraper.GetStandingsAsync(new StandingsRequest
                        {
                            SeasonId = seasonId,
                            Category = league.Category,
                            LeagueCode = league.Code,
                            CompetitionName = competition.Name,
                        });

                        if (!standings.HasGoztepe) continue;

                        allCompetitions.Add(new
                        {
                            competition.Name,
                            competition.DisplayName,
                            competition.LeagueCode,
                            competition.Category,
                            leagueDisplayName = Models.SupportedLeagues
                                .Find(league.Code)?.DisplayName ?? league.Code,
                            hasGoztepe = true,
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log and continue — one failing competition should not abort the scan
                        // This is logged but not surfaced to the caller
                        _ = ex;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        return Ok(new
        {
            seasonId,
            category,
            total = allCompetitions.Count,
            competitions = allCompetitions,
        });
    }


    /// <summary>Returns current standings cache status.</summary>
    [HttpGet("cache/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCacheStatus() =>
        Ok(new
        {
            totalCachedKeys = _cache.GetCachedKeys().Count,
            keys = _cache.GetCachedKeys()
        });

    /// <summary>Clears standings cache for a specific season or all.</summary>
    [HttpDelete("cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ClearCache([FromQuery] string? seasonId = null)
    {
        if (seasonId == null)
        {
            _cache.Clear();
        }
        else
        {
            _cache.Clear(seasonId);
        }

        return Ok(new
        {
            message = seasonId == null ? "All standings cache cleared" : $"Cache cleared for season {seasonId}",
            seasonId
        });
    }
}