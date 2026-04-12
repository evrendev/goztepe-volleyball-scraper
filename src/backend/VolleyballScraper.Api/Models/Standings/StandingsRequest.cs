namespace VolleyballScraper.Api.Models.Standings;

/// <summary>Request parameters for fetching standings and match results.</summary>
public class StandingsRequest
{
    /// <summary>Season identifier.</summary>
    /// <example>2025-2026</example>
    public string SeasonId { get; set; } = AppConstants.SeasonId;

    /// <summary>Category code (e.g. GK, KK, MdK).</summary>
    /// <example>GK</example>
    public string Category { get; set; } = "";

    /// <summary>League code (e.g. GKSL, MDK1L).</summary>
    /// <example>GKSL</example>
    public string LeagueCode { get; set; } = "";

    /// <summary>
    /// Name of the competition returned by the competitions endpoint.
    /// </summary>
    /// <example>2025-2026 GKSL Group A</example>
    public string CompetitionName { get; set; } = "";
}