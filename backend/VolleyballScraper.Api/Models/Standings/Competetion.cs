namespace VolleyballScraper.Api.Models.Standings;

/// <summary>
/// Represents a competition (yarışma) entry from the standings page dropdown.
/// Each competition corresponds to a specific phase (group stage, quarter-final, etc.)
/// within a league for a given season.
/// </summary>
public class Competition
{
    /// <summary>
    /// Human-readable competition name as shown on the federation site.
    /// </summary>
    /// <example>17345*8FF84B09-1B9E-4C7D-8BF7-F9EC67B4D5E2</example>
    public string Name { get; set; } = "";

    /// <summary>
    /// Human-readable competition title as shown on the federation site.
    /// </summary>
    /// <example>Süper Lig Genç Kızlar - Çf &amp; Yf Elm. Sıralama A Gr</example>
    public string Title { get; set; } = "";

    /// <summary>
    /// League code this competition belongs to (e.g. GKSL, MDK1L).
    /// Derived from the request context, not from the site response.
    /// </summary>
    /// <example>GKSL</example>
    public string LeagueCode { get; set; } = "";

    /// <summary>
    /// Category code this competition belongs to (e.g. GK, KK, MdK).
    /// </summary>
    /// <example>GK</example>
    public string Category { get; set; } = "";

    /// <summary>
    /// Indicates whether Göztepe participates in this competition.
    /// Populated after fetching the standings for this competition.
    /// </summary>
    public bool HasGoztepe { get; set; }
}