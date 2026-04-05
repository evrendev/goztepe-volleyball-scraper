namespace VoleybolScraper.Api.Models;

public class Mac
{
    public string Tarih { get; set; } = "";
    public string Saat { get; set; } = "";
    public string Salon { get; set; } = "";
    public string EvSahibi { get; set; } = "";
    public string Misafir { get; set; } = "";
    public string Skor { get; set; } = ""; // Oynanmadıysa boş
    public string Kume { get; set; } = "";
    public string Grup { get; set; } = "";
    public string League { get; set; } = "";
}