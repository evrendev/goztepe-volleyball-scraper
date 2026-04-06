namespace VolleyballScraper.Api.Models;

/// <summary>Request body for fetching fixture data.</summary>
public class FixtureRequest
{
    /// <summary>Season identifier.</summary>
    /// <example>2025-2026</example>
    public string SeasonId { get; set; } = "2025-2026";

    // Fixed server-side — cannot be overridden
    public string OrganisationId { get; set; } = "662";
    public string Gender { get; set; } = "B";

    /// <summary>
    /// League codes to fetch. Leave empty for all leagues.
    /// Supported: GKSL, YKSL, KKSL, MDKSL, MDK1L, MDK2L, KK1L, KK2L, YK2L
    /// </summary>
    /// <example>["GKSL", "YKSL"]</example>
    public List<string> Leagues { get; set; } = [];

    /// <summary>Filter by division code (Küme). E.g. "MDKSL", "KK1L"</summary>
    /// <example>MDKSL</example>
    public string? Division { get; set; }

    /// <summary>Filter by category (Ktg.). E.g. "GK", "KK", "MdK", "YK"</summary>
    /// <example>MdK</example>
    public string? Category { get; set; }

    /// <summary>Filter by match type (Tür). E.g. "KL", "CF", "YF", "FI"</summary>
    /// <example>KL</example>
    public string? MatchType { get; set; }

    /// <summary>Filter by group (Gr.). E.g. "A", "B", "C"</summary>
    /// <example>A</example>
    public string? Group { get; set; }

    /// <summary>Filter by round (Dv.). E.g. "1D", "2D"</summary>
    /// <example>1D</example>
    public string? Round { get; set; }

    /// <summary>Filter by week number (Hft.). E.g. "1", "5"</summary>
    /// <example>1</example>
    public string? Week { get; set; }
}