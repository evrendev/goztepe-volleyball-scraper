namespace VolleyballScraper.Api.Models;

/// <summary>Request body for fetching fixture data.</summary>
public class FixtureRequest
{
    /// <example>2025-2026</example>
    public string SeasonId { get; set; } = "2024-2025";

    // Fixed server-side
    public string OrganizationId { get; set; } = "662";
    public string Gender { get; set; } = "B";

    /// <summary>
    /// League codes to fetch. Leave empty for all leagues.
    /// Supported: GKSL, YKSL, KKSL, MDKSL, MDK1L, MDK2L, KK1L, KK2L, YK2L
    /// </summary>
    /// <example>["GKSL", "YKSL"]</example>
    public List<string> Leagues { get; set; } = [];
}