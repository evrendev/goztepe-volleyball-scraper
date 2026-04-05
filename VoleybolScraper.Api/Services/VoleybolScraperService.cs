using HtmlAgilityPack;
using VoleybolScraper.Api.Models;

namespace VoleybolScraper.Api.Services;

public class VoleybolScraperService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VoleybolScraperService> _logger;
    private const string BaseUrl = "https://izmir.voleyboliltemsilciligi.com/Fiksturler";

    public VoleybolScraperService(IHttpClientFactory httpClientFactory,
                                   ILogger<VoleybolScraperService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<Mac>> GetMaclarAsync(FiksturRequest request)
    {
        // Hiç küme seçilmemişse desteklenen tüm kümeleri çek
        var targetLeagues = request.Leagues.Count > 0
            ? request.Leagues
            : SupportedLeagues.All.Select(l => l.Code).Distinct().ToList();

        var client = _httpClientFactory.CreateClient("VoleybolClient");

        // Her küme için session'ı yeniden başlatmamak adına
        // cookie + viewstate'i bir kere al, her küme için yeniden kullan
        var allMatches = new List<Mac>();

        foreach (var leagueCode in targetLeagues)
        {
            var league = SupportedLeagues.Find(leagueCode);
            if (league == null)
            {
                _logger.LogWarning("Bilinmeyen kume kodu: {code}", leagueCode);
                continue;
            }

            _logger.LogInformation("Kume cekiliyor: {code}", leagueCode);

            try
            {
                var matches = await FetchLeagueAsync(client, request, league);
                allMatches.AddRange(matches);
                _logger.LogInformation("{code} → {count} mac", leagueCode, matches.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kume cekilemedi: {code}", leagueCode);
            }

            // Siteye aşırı yük bindirmemek için kısa bekleme
            await Task.Delay(300);
        }

        // Tarihe göre sırala
        return allMatches
            .OrderBy(m => ParseDate(m.Tarih))
            .ThenBy(m => m.Saat)
            .ToList();
    }

    private async Task<List<Mac>> FetchLeagueAsync(
        HttpClient client, FiksturRequest request, LeagueDefinition league)
    {
        var (viewState, viewStateGen, cookie) = await GetViewStateAsync(client);
        _logger.LogInformation("[{code}] VIEWSTATE alindi.", league.Code);

        // Adım 1: Sezon
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlsezon",
            extraFields: new()
            {
                ["ctl00$icerik$ddlsezon"] = request.SezonId,
                ["ctl00$icerik$ddlsbe"] = request.Gender,
                ["ctl00$icerik$ddlskategori"] = "0",
                ["ctl00$icerik$ddlskume"] = "0",
                ["ctl00$icerik$ddlsturu"] = "0",
                ["ctl00$icerik$ddlsgrubu"] = "0",
                ["ctl00$icerik$ddlskurumadi"] = "0",
                ["ctl00$icerik$ddlstakim"] = "0",
                ["ctl00$icerik$ddlsyarismaadi"] = "0",
            });
        _logger.LogInformation("[{code}] Sezon secildi: {season}", league.Code, request.SezonId);

        // Adım 2: Kategori — ham yanıtı logla
        var categoryRaw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskategori",
            extraFields: new()
            {
                ["ctl00$icerik$ddlsezon"] = request.SezonId,
                ["ctl00$icerik$ddlsbe"] = request.Gender,
                ["ctl00$icerik$ddlskategori"] = league.Category,
                ["ctl00$icerik$ddlskume"] = "0",
                ["ctl00$icerik$ddlsturu"] = "0",
                ["ctl00$icerik$ddlsgrubu"] = "0",
                ["ctl00$icerik$ddlskurumadi"] = "0",
                ["ctl00$icerik$ddlstakim"] = "0",
                ["ctl00$icerik$ddlsyarismaadi"] = "0",
            });

        // Küme listesinin dolup dolmadığını kontrol et
        var hasLeagueCode = categoryRaw.Contains(league.Code);
        _logger.LogInformation(
            "[{code}] Kategori POST sonrasi kume listesinde {code} var mi: {result}. Yanit(500): {raw}",
            league.Code, league.Code, hasLeagueCode,
            categoryRaw[..Math.Min(500, categoryRaw.Length)]);

        var newVsFromCat = ExtractHiddenField(categoryRaw, "__VIEWSTATE");
        var newGenFromCat = ExtractHiddenField(categoryRaw, "__VIEWSTATEGENERATOR");
        if (!string.IsNullOrEmpty(newVsFromCat)) viewState = newVsFromCat;
        if (!string.IsNullOrEmpty(newGenFromCat)) viewStateGen = newGenFromCat;

        // Adım 3: Küme
        var kumeRaw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskume",
            extraFields: new()
            {
                ["ctl00$icerik$ddlsezon"] = request.SezonId,
                ["ctl00$icerik$ddlsbe"] = request.Gender,
                ["ctl00$icerik$ddlskategori"] = league.Category,
                ["ctl00$icerik$ddlskume"] = league.Code,
                ["ctl00$icerik$ddlsturu"] = "0",
                ["ctl00$icerik$ddlsgrubu"] = "0",
                ["ctl00$icerik$ddlskurumadi"] = "0",
                ["ctl00$icerik$ddlstakim"] = "0",
                ["ctl00$icerik$ddlsyarismaadi"] = "0",
            });

        // Kurum listesinin dolup dolmadığını kontrol et
        var hasKurumId = kumeRaw.Contains($"value=\"{request.KurumId}\"");
        _logger.LogInformation(
            "[{code}] Kume POST sonrasi kurumId={kurumId} listede var mi: {result}",
            league.Code, request.KurumId, hasKurumId);

        var newVsFromKume = ExtractHiddenField(kumeRaw, "__VIEWSTATE");
        var newGenFromKume = ExtractHiddenField(kumeRaw, "__VIEWSTATEGENERATOR");
        if (!string.IsNullOrEmpty(newVsFromKume)) viewState = newVsFromKume;
        if (!string.IsNullOrEmpty(newGenFromKume)) viewStateGen = newGenFromKume;

        // Adım 4: Kurum → maçlar
        var matchesRaw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskurumadi",
            extraFields: new()
            {
                ["ctl00$icerik$ddlsezon"] = request.SezonId,
                ["ctl00$icerik$ddlsbe"] = request.Gender,
                ["ctl00$icerik$ddlskategori"] = league.Category,
                ["ctl00$icerik$ddlskume"] = league.Code,
                ["ctl00$icerik$ddlsturu"] = "0",
                ["ctl00$icerik$ddlsgrubu"] = "0",
                ["ctl00$icerik$ddlskurumadi"] = request.KurumId,
                ["ctl00$icerik$ddlstakim"] = "0",
                ["ctl00$icerik$ddlsyarismaadi"] = "0",
            });

        var kayitSayisi = ExtractRecordCount(matchesRaw);
        _logger.LogInformation(
            "[{code}] Mac POST tamamlandi. Kayit sayisi: {count}",
            league.Code, kayitSayisi);

        var matches = ParseMatches(matchesRaw);

        foreach (var m in matches)
            m.League = league.DisplayName;

        return matches;
    }

    // Sayfadaki "Kayit" alanındaki kırmızı sayıyı çek
    private static string ExtractRecordCount(string delta)
    {
        var html = ExtractUpdatePanelHtml(delta);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.GetElementbyId("icerik_lblkayitsayisi")?.InnerText.Trim() ?? "?";
    }
    // ── HTTP Helpers ────────────────────────────────────────────────

    private async Task<(string vs, string gen)> PostStepAsync(
        HttpClient client, string cookie,
        string viewState, string viewStateGen,
        string eventTarget, Dictionary<string, string> extraFields)
    {
        var raw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen, eventTarget, extraFields);

        var newVs = ExtractHiddenField(raw, "__VIEWSTATE");
        var newGen = ExtractHiddenField(raw, "__VIEWSTATEGENERATOR");

        return (
            string.IsNullOrEmpty(newVs) ? viewState : newVs,
            string.IsNullOrEmpty(newGen) ? viewStateGen : newGen
        );
    }

    private async Task<string> PostStepRawAsync(
        HttpClient client, string cookie,
        string viewState, string viewStateGen,
        string eventTarget, Dictionary<string, string> extraFields)
    {
        var formData = new Dictionary<string, string>
        {
            ["ctl00$ScriptManager1"] = $"ctl00$icerik$UpdatePanel|{eventTarget}",
            ["ctl00$mail"] = "",
            ["ctl00$password"] = "",
            ["ctl00$icerik$txtmemberuser"] = "",
            ["ctl00$icerik$txtkayitid"] = "",
            ["ctl00$icerik$txtpageno"] = "",
            ["ctl00$icerik$txtilid"] = "35",
            ["ctl00$icerik$txtyetkiseviyesi"] = "",
            ["ctl00$icerik$txtgun"] = "",
            ["ctl00$icerik$txtmacide"] = "",
            ["ctl00$icerik$txtyil"] = "",
            ["ctl00$icerik$pageno"] = "",
            ["ctl00$icerik$ddlSil"] = "35",
            ["ctl00$icerik$txttarih"] = "",
            ["ctl00$icerik$txtbitistrh"] = "",
            ["__LASTFOCUS"] = "",
            ["__EVENTTARGET"] = eventTarget,
            ["__EVENTARGUMENT"] = "",
            ["__VIEWSTATE"] = viewState,
            ["__VIEWSTATEGENERATOR"] = viewStateGen,
            ["__VIEWSTATEENCRYPTED"] = "",
            ["__ASYNCPOST"] = "true",
        };

        foreach (var kv in extraFields)
            formData[kv.Key] = kv.Value;

        var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
        {
            Content = new FormUrlEncodedContent(formData)
        };

        msg.Headers.TryAddWithoutValidation("Cookie", cookie);
        msg.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
        msg.Headers.TryAddWithoutValidation("X-MicrosoftAjax", "Delta=true");
        msg.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");

        var resp = await client.SendAsync(msg);
        return await resp.Content.ReadAsStringAsync();
    }

    private async Task<(string viewState, string generator, string cookie)>
        GetViewStateAsync(HttpClient client)
    {
        var response = await client.GetAsync(BaseUrl);
        var html = await response.Content.ReadAsStringAsync();

        var cookieParts = new List<string>();
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            foreach (var c in cookies)
                cookieParts.Add(c.Split(';')[0]);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return (
            doc.GetElementbyId("__VIEWSTATE")
               ?.GetAttributeValue("value", "") ?? "",
            doc.GetElementbyId("__VIEWSTATEGENERATOR")
               ?.GetAttributeValue("value", "") ?? "",
            string.Join("; ", cookieParts)
        );
    }

    // ── Parse ───────────────────────────────────────────────────────

    private List<Mac> ParseMatchesFromHtml(string html, string leagueDisplay)
    {
        var matches = new List<Mac>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var table = doc.GetElementbyId("icerik_gvliste")
                    ?? doc.DocumentNode.SelectSingleNode("//table");
        if (table == null) return matches;

        var rows = table.SelectNodes(".//tr");
        if (rows == null || rows.Count < 2) return matches;

        foreach (var row in rows.Skip(1))
        {
            // Sayfalama satırını class'tan anla
            if (row.GetAttributeValue("class", "").Contains("pagination")) continue;

            var cols = row.SelectNodes(".//td");
            if (cols == null || cols.Count < 6) continue;

            var tarih = Clean(cols[0].InnerText);
            if (!IsValidDate(tarih)) continue;

            // Skor: col[5] = C kolonu (B/E gösterge, gerçek skor değil)
            // Gerçek skor geldiğinde "3-1" formatında olur, "B" değil
            var skorRaw = cols.Count > 5 ? Clean(cols[5].InnerText) : "";
            var skor = (skorRaw == "B" || skorRaw == "E") ? "" : skorRaw;

            matches.Add(new Mac
            {
                Tarih = tarih,
                Saat = cols.Count > 1 ? Clean(cols[1].InnerText) : "",
                Salon = cols.Count > 2 ? Clean(cols[2].InnerText) : "",
                EvSahibi = cols.Count > 3 ? Clean(cols[3].InnerText) : "",
                Misafir = cols.Count > 4 ? Clean(cols[4].InnerText) : "",
                Skor = skor,
                Kume = cols.Count > 6 ? Clean(cols[6].InnerText) : "",
                Grup = cols.Count > 9 ? Clean(cols[9].InnerText) : "",
                League = leagueDisplay,
            });
        }

        return matches;
    }

    // Sayfa numaralarını çek — pagination satırından
    private static List<int> ExtractPageNumbers(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var pages = new List<int> { 1 }; // İlk sayfa her zaman var

        var paginationRow = doc.DocumentNode
            .SelectSingleNode("//tr[contains(@class,'pagination')]");
        if (paginationRow == null) return pages;

        var links = paginationRow.SelectNodes(".//a");
        if (links != null)
        {
            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", "");
                var match = System.Text.RegularExpressions.Regex.Match(href, @"Page\$(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var num))
                    if (!pages.Contains(num))
                        pages.Add(num);
            }
        }

        return pages.OrderBy(p => p).ToList();
    }

    // ParseMatches artık kullanılmıyor, eskiyle uyumluluk için yönlendir
    private List<Mac> ParseMatches(string rawResponse) =>
        ParseMatchesFromHtml(ExtractUpdatePanelHtml(rawResponse), "");

    // ── Helpers ─────────────────────────────────────────────────────

    private static bool IsPaginationRow(string text) =>
        // Gerçek sayfalama: "12345678910..." gibi ardışık rakamlar
        // "18.09.2025" gibi tarihler değil — noktasız hali kontrol etme
        !string.IsNullOrWhiteSpace(text) &&
        !text.Contains('.') &&          // Tarihler nokta içerir, sayfalama satırları içermez
        text.Replace(" ", "").All(char.IsDigit);

    private static bool IsValidDate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        // GG.AA.YYYY formatı
        return System.Text.RegularExpressions.Regex.IsMatch(
            text, @"^\d{2}\.\d{2}\.\d{4}$");
    }

    private static DateTime ParseDate(string text)
    {
        if (DateTime.TryParseExact(text, "dd.MM.yyyy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var dt))
            return dt;
        return DateTime.MaxValue;
    }

    private static string ExtractHiddenField(string delta, string fieldName)
    {
        var marker = $"|hiddenField|{fieldName}|";
        var idx = delta.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return "";

        var start = idx + marker.Length;
        var end = delta.IndexOf('|', start);
        return end < 0 ? delta[start..] : delta[start..end];
    }

    private static string ExtractUpdatePanelHtml(string delta)
    {
        if (string.IsNullOrWhiteSpace(delta)) return delta;

        // Delta formatı: "boyut|updatePanel|panelId|içerik|boyut|tip|..."
        // Regex ile boyut|updatePanel|panelId| kalıbını bul
        var match = System.Text.RegularExpressions.Regex.Match(
            delta,
            @"(\d+)\|updatePanel\|[^|]+\|",
            System.Text.RegularExpressions.RegexOptions.None);

        if (!match.Success) return delta;

        // Boyutu al
        if (!int.TryParse(match.Groups[1].Value, out var size)) return delta;

        // İçerik match'in bittiği yerden başlar
        var contentStart = match.Index + match.Length;

        if (contentStart + size > delta.Length)
            size = delta.Length - contentStart;

        return delta.Substring(contentStart, size);
    }

    private static string Clean(string text) =>
        System.Net.WebUtility.HtmlDecode(text)
              .Trim()
              .Replace("\r", "")
              .Replace("\n", " ")
              .Replace("\t", " ");
}