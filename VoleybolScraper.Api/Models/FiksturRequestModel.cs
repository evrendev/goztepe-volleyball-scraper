namespace VoleybolScraper.Api.Models;

public class FiksturRequest
{
    public string SezonId { get; set; } = "2024-2025";

    // Sabit: Göztepe = 662, Kadın = B
    public string KurumId { get; set; } = "662";
    public string Gender { get; set; } = "B";

    // Hangi kümeler çekilsin
    public List<string> Leagues { get; set; } = [];
}