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
    /// Numeric competition ID returned by the competitions endpoint.
    /// </summary>
    /// <example>19285</example>
    public string CompetitionId { get; set; } = "";

    /// <summary>
    /// Competition GUID key returned by the competitions endpoint.
    /// </summary>
    /// <example>5DB65D03-09DD-4B8E-99E3-7940EEA1C725</example>
    public string CompetitionKey { get; set; } = "";
}