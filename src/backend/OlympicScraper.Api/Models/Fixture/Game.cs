namespace OlympicScraper.Api.Models;

/// <summary>Represents a single volleyball fixture.</summary>
public class Game
{
    /// <example>18.09.2025</example>
    public string Date { get; set; } = "";

    /// <example>18:30</example>
    public string Time { get; set; } = "";

    /// <example>Gaziemir S.S.</example>
    public string Venue { get; set; } = "";

    /// <example>Göztepe</example>
    public string HomeTeam { get; set; } = "";

    /// <example>Arkas</example>
    public string AwayTeam { get; set; } = "";

    /// <summary>Empty string if the game has not been played yet.</summary>
    /// <example></example>
    public string Score { get; set; } = "";

    /// <summary>Küme (division code). E.g. GKSL, MDK1L</summary>
    /// <example>GKSL</example>
    public string Division { get; set; } = "";

    /// <summary>Kategori. E.g. GK, KK, MdK, YK</summary>
    /// <example>GK</example>
    public string Category { get; set; } = "";

    /// <summary>Tür (match type). E.g. KL, CF, YF, FI</summary>
    /// <example>KL</example>
    public string MatchType { get; set; } = "";

    /// <summary>Grup. E.g. A, B, C</summary>
    /// <example>A</example>
    public string Group { get; set; } = "";

    /// <summary>Devre (round). E.g. 1D, 2D</summary>
    /// <example>1D</example>
    public string Round { get; set; } = "";

    /// <summary>Hafta (week number)</summary>
    /// <example>1</example>
    public string Week { get; set; } = "";

    /// <example>Genç Kızlar Süper Lig</example>
    public string League { get; set; } = "";
}