namespace OlympicScraper.Api.Models.Volleyball.Standings;

/// <summary>
/// Full standings response for a single competition,
/// combining the standings table and the match result list.
/// </summary>
public class Response
{
    /// <summary>Numeric competition identifier.</summary>
    /// <example>19285</example>
    public string CompetitionId { get; set; } = "";

    /// <summary>Human-readable competition name.</summary>
    /// <example>Süper Lig Genç Kızlar - Çf &amp; Yf Elm. Sıralama A Gr</example>
    public string CompetitionName { get; set; } = "";

    /// <summary>Season identifier this standings data belongs to.</summary>
    /// <example>2025-2026</example>
    public string SeasonId { get; set; } = "";

    /// <summary>
    /// Indicates whether Göztepe appears in the standings table for this competition.
    /// </summary>
    public bool HasGoztepe { get; set; }

    /// <summary>Ordered list of team standings rows.</summary>
    public List<Row> Standings { get; set; } = [];

    /// <summary>
    /// List of all matches in this competition,
    /// including both played results and upcoming fixtures.
    /// </summary>
    public List<GameResult> Games { get; set; } = [];
}