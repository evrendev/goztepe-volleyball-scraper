namespace OlympicScraper.Api.Models.Standings;

/// <summary>
/// Represents a single match result row from the competition match list
/// shown below the standings table on the federation site.
/// </summary>
public class GameResult
{
    /// <summary>
    /// Sequential match number within the competition (S.No).
    /// </summary>
    /// <example>1</example>
    public int RowNo { get; set; }

    /// <summary>Game date in DD.MM.YYYY format.</summary>
    /// <example>27.03.2026</example>
    public string Date { get; set; } = "";

    /// <summary>Game start time in HH:mm format.</summary>
    /// <example>20:00</example>
    public string Time { get; set; } = "";

    /// <summary>Name of the venue where the match is played.</summary>
    /// <example>Vali Hüseyin Öğütcen</example>
    public string Venue { get; set; } = "";

    /// <summary>Home team name (Ev Sahibi — A).</summary>
    /// <example>Mavişehir</example>
    public string HomeTeam { get; set; } = "";

    /// <summary>Away team name (Misafir — B).</summary>
    /// <example>Smyrna Yıldızı</example>
    public string AwayTeam { get; set; } = "";

    /// <summary>
    /// Sets won by the home team.
    /// Zero if the match has not been played yet.
    /// </summary>
    /// <example>3</example>
    public int HomeScore { get; set; }

    /// <summary>
    /// Sets won by the away team.
    /// Zero if the match has not been played yet.
    /// </summary>
    /// <example>2</example>
    public int AwayScore { get; set; }

    /// <summary>
    /// Individual set scores in the format "(25-22) (23-25) (25-23) (20-25) (15-9)".
    /// Empty string if the match has not been played yet.
    /// </summary>
    /// <example>(25-22) (23-25) (25-23) (20-25) (15-9)</example>
    public string SetResults { get; set; } = "";

    /// <summary>
    /// Indicates whether the match has been played and has a result.
    /// False for future fixtures without a score.
    /// </summary>
    public bool IsPlayed { get; set; }

    /// <summary>
    /// Indicates whether Göztepe is participating in this match
    /// (either as home or away team).
    /// </summary>
    public bool IsGoztepe { get; set; }
}