namespace VolleyballScraper.Api.Models;

public class FixtureRequest
{
    public string SeasonId { get; set; } = "2024-2025";

    // Fixed: Göztepe = 662, Women = B
    public string OrganizationId { get; set; } = "662";
    public string Gender { get; set; } = "B";

    // Which leagues to fetch
    public List<string> Leagues { get; set; } = [];
}