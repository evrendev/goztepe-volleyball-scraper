namespace VolleyballScraper.Api.Models;

public class Game
{
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public string Venue { get; set; } = "";
    public string HomeTeam { get; set; } = "";
    public string AwayTeam { get; set; } = "";
    public string Score { get; set; } = ""; // Empty if not played
    public string Division { get; set; } = "";
    public string Group { get; set; } = "";
    public string League { get; set; } = "";
}