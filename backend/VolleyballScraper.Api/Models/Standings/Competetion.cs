namespace VolleyballScraper.Api.Models.Standings;

/// <summary>
/// Represents a competition (yarışma) entry from the standings page dropdown.
/// Each competition corresponds to a specific phase (group stage, quarter-final, etc.)
/// within a league for a given season.
/// </summary>
public class Competition
{
    /// <summary>
    /// Numeric competition identifier from the federation site.
    /// Used as part of the POST payload to fetch standings.
    /// </summary>
    /// <example>19285</example>
    public string Id { get; set; } = "";

    /// <summary>
    /// GUID key associated with the competition on the federation site.
    /// Combined with <see cref="Id"/> to form the raw dropdown value.
    /// </summary>
    /// <example>5DB65D03-09DD-4B8E-99E3-7940EEA1C725</example>
    public string Key { get; set; } = "";

    /// <summary>
    /// Human-readable competition name as shown on the federation site.
    /// </summary>
    /// <example>Süper Lig Genç Kızlar - Çf &amp; Yf Elm. Sıralama A Gr</example>
    public string Name { get; set; } = "";

    /// <summary>
    /// Raw dropdown option value in the format <c>id*key</c>.
    /// Sent as-is in the POST payload when selecting a competition.
    /// </summary>
    /// <example>19285*5DB65D03-09DD-4B8E-99E3-7940EEA1C725</example>
    public string RawValue { get; set; } = "";

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