namespace OlympicScraper.Api.Models.Standings;

/// <summary>Request parameters for fetching the competition list.</summary>
public class CompetitionRequest
{
    /// <summary>Season identifier.</summary>
    /// <example>2025-2026</example>
    public string SeasonId { get; set; } = AppConstants.SeasonId;

    /// <summary>
    /// Category code. Must match a valid category on the federation site.
    /// Supported: GK, YK, KK, MdK, MnK
    /// </summary>
    /// <example>GK</example>
    public string Category { get; set; } = "";

    /// <summary>
    /// League code. Must match a valid league code in <see cref="SupportedLeagues"/>.
    /// </summary>
    /// <example>GKSL</example>
    public string LeagueCode { get; set; } = "";
}