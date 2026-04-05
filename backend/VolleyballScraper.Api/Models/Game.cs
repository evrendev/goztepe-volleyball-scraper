namespace VolleyballScraper.Api.Models;

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

    /// <example>GKSL</example>
    public string Division { get; set; } = "";

    /// <example>A</example>
    public string Group { get; set; } = "";

    /// <example>Genç Kızlar Süper Lig</example>
    public string League { get; set; } = "";
}