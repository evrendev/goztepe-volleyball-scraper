namespace OlympicScraper.Api.Models.Standings;

/// <summary>
/// Represents a single team row in the standings table (puan durumu).
/// Column mapping matches the federation site table headers.
/// </summary>
public class StandingsRow
{
    /// <summary>Current rank of the team in the standings.</summary>
    /// <example>1</example>
    public int Rank { get; set; }

    /// <summary>Team name as displayed on the federation site.</summary>
    /// <example>Göztepe</example>
    public string TeamName { get; set; } = "";

    /// <summary>
    /// Absolute URL of the team's logo image hosted on the federation site.
    /// Empty string if no logo is available.
    /// </summary>
    /// <example>https://izmir.voleyboliltemsilciligi.com/Uploads/KulupKurum/Logo/2173.png</example>
    public string LogoUrl { get; set; } = "";

    /// <summary>Number of matches played (O — Oynadığı).</summary>
    /// <example>2</example>
    public int Played { get; set; }

    /// <summary>Number of matches won (G — Galibiyet).</summary>
    /// <example>2</example>
    public int Won { get; set; }

    /// <summary>Number of matches lost (M — Mağlubiyet).</summary>
    /// <example>0</example>
    public int Lost { get; set; }

    /// <summary>Total sets won across all matches (A — Alınan Set).</summary>
    /// <example>6</example>
    public int SetsWon { get; set; }

    /// <summary>Total sets lost across all matches (V — Verilen Set).</summary>
    /// <example>1</example>
    public int SetsLost { get; set; }

    /// <summary>League points (P — Puan).</summary>
    /// <example>5</example>
    public int Points { get; set; }

    /// <summary>
    /// Set average ratio: sets won / sets lost (SAV — Set Averajı).
    /// Displayed as a decimal string on the site.
    /// </summary>
    /// <example>2.000</example>
    public string SetAverage { get; set; } = "";

    /// <summary>Total individual points (rallies) won (ASP — Alınan Sayı Puanı).</summary>
    /// <example>204</example>
    public int PointsWon { get; set; }

    /// <summary>Total individual points (rallies) lost (VSP — Verilen Sayı Puanı).</summary>
    /// <example>177</example>
    public int PointsLost { get; set; }

    /// <summary>
    /// Point average ratio: points won / points lost (SPAV — Sayı Puanı Averajı).
    /// </summary>
    /// <example>1.153</example>
    public int PointAverage { get; set; }

    /// <summary>Number of matches won 3-0 (straight sets).</summary>
    public int W30 { get; set; }

    /// <summary>Number of matches won 3-1.</summary>
    public int W31 { get; set; }

    /// <summary>Number of matches won 3-2.</summary>
    public int W32 { get; set; }

    /// <summary>Number of matches lost 2-3.</summary>
    public int L23 { get; set; }

    /// <summary>Number of matches lost 1-3.</summary>
    public int L13 { get; set; }

    /// <summary>Number of matches lost 0-3.</summary>
    public int L03 { get; set; }

    /// <summary>
    /// Indicates whether this row belongs to Göztepe.
    /// Used by the frontend to highlight the club's row.
    /// </summary>
    public bool IsGoztepe { get; set; }
}