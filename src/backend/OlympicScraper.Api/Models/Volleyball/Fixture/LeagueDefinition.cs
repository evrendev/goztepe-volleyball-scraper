namespace OlympicScraper.Api.Models.Volleyball.Fixture;

public record LeagueDefinition(
    string Code,
    string Category,
    string DisplayName
);

public static class SupportedLeagues
{
    // Category codes must game exactly with <option value="..."> on the site:
    // GK, YK, KK, MdK, MnK, BB
    public static readonly List<LeagueDefinition> All =
    [
        new("GKSL",  "GK",  "Genç Kızlar Süper Lig"),
        new("KKSL",  "KK",  "Küçük Kız Süper Lig"),
        new("KK1L",  "KK",  "Küçük Kızlar 1. Lig"),
        new("KK2L",  "KK",  "Küçük Kız 2. Lig"),
        new("MDKSL", "MdK", "Midi Kız Süper Lig"),
        new("MDK1L", "MdK", "Midi Kızlar 1. Lig"),
        new("MDK2L", "MdK", "Midi Kız 2. Lig"),
        new("MDKGL", "MdK", "Midi Kızlar Gelişim Ligi"),
        new("MNK1L", "MnK", "Mini Kızlar 1. Lig"),
        new("MNK2L", "MnK", "Mini Kızlar 2. Lig"),
        new("YKSL",  "YK",  "Yıldız Kız Süper Lig"),
        new("YK2L",  "YK",  "Yıldız Kızlar 2. Lig"),
    ];

    public static LeagueDefinition? Find(string code) =>
        All.FirstOrDefault(l => l.Code == code);
}