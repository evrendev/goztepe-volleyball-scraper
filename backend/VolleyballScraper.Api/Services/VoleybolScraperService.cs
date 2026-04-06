using HtmlAgilityPack;
using VolleyballScraper.Api.Models;

namespace VolleyballScraper.Api.Services;

public class VolleyballScraperService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VolleyballScraperService> _logger;
    private readonly GameCacheService _cache;
    private const string BaseUrl = "https://izmir.voleyboliltemsilciligi.com/Fiksturler";

    public VolleyballScraperService(
        IHttpClientFactory httpClientFactory,
        ILogger<VolleyballScraperService> logger,
        GameCacheService cache)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _cache = cache;
    }

    // Max concurrent fetches to avoid hammering the external site
    private static readonly SemaphoreSlim _fetchSemaphore = new(3, 3);

    public async Task<List<Game>> GetGamesAsync(FixtureRequest request,
                                                 bool forceRefresh = false)
    {
        var targetLeagues = request.Leagues.Count > 0
            ? request.Leagues
            : SupportedLeagues.All.Select(l => l.Code).Distinct().ToList();

        var allGames = new List<Game>();
        var cacheHits = 0;
        var cacheMisses = 0;

        // Sequential — not parallel — to avoid overwhelming the external site
        foreach (var leagueCode in targetLeagues)
        {
            var league = SupportedLeagues.Find(leagueCode);
            if (league == null) continue;

            var cacheKey = _cache.BuildKey(request.SeasonId, leagueCode);

            if (!forceRefresh && _cache.TryGet(cacheKey, out var cached))
            {
                allGames.AddRange(cached);
                cacheHits++;
                continue;
            }

            cacheMisses++;
            _logger.LogInformation("Fetching from site: {code}", leagueCode);

            try
            {
                var client = _httpClientFactory.CreateClient("VolleyballClient");
                var games = await FetchLeagueAsync(client, request, league);
                _cache.Set(cacheKey, games);
                allGames.AddRange(games);

                // Brief pause between leagues to be polite to the site
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch: {code}", leagueCode);
            }
        }

        _logger.LogInformation(
            "Total: {total} games | Cache hits: {hits} | Fetched: {misses}",
            allGames.Count, cacheHits, cacheMisses);

        return allGames
            .OrderBy(m => ParseDate(m.Date))
            .ThenBy(m => m.Time)
            .ToList();
    }
    private async Task<List<Game>> FetchLeagueAsync(
        HttpClient client, FixtureRequest request, LeagueDefinition league)
    {
        var (viewState, viewStateGen, cookie) = await GetViewStateAsync(client);
        _logger.LogInformation("[{code}] VIEWSTATE retrieved.", league.Code);

        // Step 1: Season
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlsezon",
            extraFields: new()
            {
                ["ctl00$icerik$ddlsezon"] = request.SeasonId,
                ["ctl00$icerik$ddlsbe"] = request.Gender,
                ["ctl00$icerik$ddlskategori"] = "0",
                ["ctl00$icerik$ddlskume"] = "0",
                ["ctl00$icerik$ddlsturu"] = "0",
                ["ctl00$icerik$ddlsgrubu"] = "0",
                ["ctl00$icerik$ddlskurumadi"] = "0",
                ["ctl00$icerik$ddlstakim"] = "0",
                ["ctl00$icerik$ddlsyarismaadi"] = "0",
            });
        _logger.LogInformation("[{code}] Season selected: {season}", league.Code, request.SeasonId);

        // Step 2: Category — log raw response
        var categoryRaw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskategori",
            extraFields: new()
            {
                ["ctl00$icerik$ddlsezon"] = request.SeasonId,
                ["ctl00$icerik$ddlsbe"] = request.Gender,
                ["ctl00$icerik$ddlskategori"] = league.Category,
                ["ctl00$icerik$ddlskume"] = "0",
                ["ctl00$icerik$ddlsturu"] = "0",
                ["ctl00$icerik$ddlsgrubu"] = "0",
                ["ctl00$icerik$ddlskurumadi"] = "0",
                ["ctl00$icerik$ddlstakim"] = "0",
                ["ctl00$icerik$ddlsyarismaadi"] = "0",
            });

        // Check if league list is populated
        var hasLeagueCode = categoryRaw.Contains(league.Code);
        _logger.LogInformation(
            "[{code}] After category POST, is {code} in league list: {result}. Response(500): {raw}",
            league.Code, league.Code, hasLeagueCode,
            categoryRaw[..Math.Min(500, categoryRaw.Length)]);

        var newVsFromCat = ExtractHiddenField(categoryRaw, "__VIEWSTATE");
        var newGenFromCat = ExtractHiddenField(categoryRaw, "__VIEWSTATEGENERATOR");
        if (!string.IsNullOrEmpty(newVsFromCat)) viewState = newVsFromCat;
        if (!string.IsNullOrEmpty(newGenFromCat)) viewStateGen = newGenFromCat;

        // Step 3: League
        var leagueRaw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskume",
            extraFields: new()
            {
                ["ctl00$icerik$ddlsezon"] = request.SeasonId,
                ["ctl00$icerik$ddlsbe"] = request.Gender,
                ["ctl00$icerik$ddlskategori"] = league.Category,
                ["ctl00$icerik$ddlskume"] = league.Code,
                ["ctl00$icerik$ddlsturu"] = "0",
                ["ctl00$icerik$ddlsgrubu"] = "0",
                ["ctl00$icerik$ddlskurumadi"] = "0",
                ["ctl00$icerik$ddlstakim"] = "0",
                ["ctl00$icerik$ddlsyarismaadi"] = "0",
            });

        // Check if organisation list is populated
        var hasOrganisationId = leagueRaw.Contains($"value=\"{request.OrganisationId}\"");
        _logger.LogInformation(
            "[{code}] After league POST, is organisationId={organisationId} in list: {result}",
            league.Code, request.OrganisationId, hasOrganisationId);

        var newVsFromLeague = ExtractHiddenField(leagueRaw, "__VIEWSTATE");
        var newGenFromLeague = ExtractHiddenField(leagueRaw, "__VIEWSTATEGENERATOR");
        if (!string.IsNullOrEmpty(newVsFromLeague)) viewState = newVsFromLeague;
        if (!string.IsNullOrEmpty(newGenFromLeague)) viewStateGen = newGenFromLeague;

        // Step 4: Organization → games (with pagination)
        var baseFields = new Dictionary<string, string>
        {
            ["ctl00$icerik$ddlsezon"] = request.SeasonId,
            ["ctl00$icerik$ddlsbe"] = request.Gender,
            ["ctl00$icerik$ddlskategori"] = league.Category,
            ["ctl00$icerik$ddlskume"] = league.Code,
            ["ctl00$icerik$ddlsturu"] = "0",
            ["ctl00$icerik$ddlsgrubu"] = "0",
            ["ctl00$icerik$ddlskurumadi"] = request.OrganisationId,
            ["ctl00$icerik$ddlstakim"] = "0",
            ["ctl00$icerik$ddlsyarismaadi"] = "0",
        };

        var firstRaw = await PostStepRawAsync(client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskurumadi", extraFields: baseFields);

        var firstHtml = ExtractUpdatePanelHtml(firstRaw);
        var allGames = new List<Game>();
        allGames.AddRange(ParseGamesFromHtml(firstHtml, league.DisplayName));

        var pageNumbers = ExtractPageNumbers(firstHtml);
        _logger.LogInformation("[{code}] Pages found: {pages}, page 1 games: {count}",
            league.Code, pageNumbers.Count, allGames.Count);

        // Update viewstate from first response
        var pageVs = ExtractHiddenField(firstRaw, "__VIEWSTATE");
        var pageGen = ExtractHiddenField(firstRaw, "__VIEWSTATEGENERATOR");
        if (!string.IsNullOrEmpty(pageVs)) viewState = pageVs;
        if (!string.IsNullOrEmpty(pageGen)) viewStateGen = pageGen;

        foreach (var pageNum in pageNumbers.Skip(1))
        {
            await Task.Delay(300);

            var pageRaw = await PostStepRawAsync(
                client, cookie, viewState, viewStateGen,
                eventTarget: "ctl00$icerik$gvliste",
                extraFields: new(baseFields)
                {
                    ["__EVENTARGUMENT"] = $"Page${pageNum}",
                });

            var pageHtml = ExtractUpdatePanelHtml(pageRaw);
            var pageGames = ParseGamesFromHtml(pageHtml, league.DisplayName);
            allGames.AddRange(pageGames);

            _logger.LogInformation("[{code}] Page {page}: {count} games",
                league.Code, pageNum, pageGames.Count);

            var newVs = ExtractHiddenField(pageRaw, "__VIEWSTATE");
            var newGen = ExtractHiddenField(pageRaw, "__VIEWSTATEGENERATOR");
            if (!string.IsNullOrEmpty(newVs)) viewState = newVs;
            if (!string.IsNullOrEmpty(newGen)) viewStateGen = newGen;
        }

        return allGames;
    }

    // Extract the red number from the "Record" field on the page
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

    private List<Game> ParseGamesFromHtml(string html, string leagueDisplay)
    {
        var games = new List<Game>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var table = doc.GetElementbyId("icerik_gvliste")
                    ?? doc.DocumentNode.SelectSingleNode("//table");
        if (table == null) return games;

        var rows = table.SelectNodes(".//tr");
        if (rows == null || rows.Count < 2) return games;

        foreach (var row in rows.Skip(1))
        {
            // Understand pagination row from class
            if (row.GetAttributeValue("class", "").Contains("pagination")) continue;

            var cols = row.SelectNodes(".//td");
            if (cols == null || cols.Count < 6) continue;

            var date = Clean(cols[0].InnerText);
            if (!IsValidDate(date)) continue;

            // Table columns (0-indexed):
            // 0=Tarih, 1=Saat, 2=Salon, 3=EvSahibi, 4=Misafir, 5=C(B/E), 6=Küme, 7=Ktg, 8=Tür, 9=Gr, 10=Dv, 11=Tv, 12=Hft
            var scoreRaw = cols.Count > 5 ? Clean(cols[5].InnerText) : "";
            var score = (scoreRaw == "B" || scoreRaw == "E") ? "" : scoreRaw;

            games.Add(new Game
            {
                Date = date,
                Time = cols.Count > 1 ? Clean(cols[1].InnerText) : "",
                Venue = cols.Count > 2 ? Clean(cols[2].InnerText) : "",
                HomeTeam = cols.Count > 3 ? Clean(cols[3].InnerText) : "",
                AwayTeam = cols.Count > 4 ? Clean(cols[4].InnerText) : "",
                Score = score,
                Division = cols.Count > 6 ? Clean(cols[6].InnerText) : "",
                Category = cols.Count > 7 ? Clean(cols[7].InnerText) : "",
                MatchType = cols.Count > 8 ? Clean(cols[8].InnerText) : "",
                Group = cols.Count > 9 ? Clean(cols[9].InnerText) : "",
                Round = cols.Count > 10 ? Clean(cols[10].InnerText) : "",
                Week = cols.Count > 12 ? Clean(cols[12].InnerText) : "",
                League = leagueDisplay,
            });
        }

        return games;
    }

    // Extract page numbers — from pagination row
    private static List<int> ExtractPageNumbers(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var pages = new List<int> { 1 }; // First page is always available

        var paginationRow = doc.DocumentNode
            .SelectSingleNode("//tr[contains(@class,'pagination')]");
        if (paginationRow == null) return pages;

        var links = paginationRow.SelectNodes(".//a");
        if (links != null)
        {
            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", "");
                var game = System.Text.RegularExpressions.Regex.Match(href, @"Page\$(\d+)");
                if (game.Success && int.TryParse(game.Groups[1].Value, out var num))
                    if (!pages.Contains(num))
                        pages.Add(num);
            }
        }

        return pages.OrderBy(p => p).ToList();
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static bool IsPaginationRow(string text) =>
        // Real pagination: consecutive numbers like "12345678910..."
        // Not dates like "18.09.2025" — check version without dots
        !string.IsNullOrWhiteSpace(text) &&
        !text.Contains('.') &&          // Dates contain dots, pagination rows don't
        text.Replace(" ", "").All(char.IsDigit);

    private static bool IsValidDate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        // DD.MM.YYYY format
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

        // Delta format: "size|updatePanel|panelId|content|size|type|..."
        // Find pattern size|updatePanel|panelId| with regex
        var game = System.Text.RegularExpressions.Regex.Match(
            delta,
            @"(\d+)\|updatePanel\|[^|]+\|",
            System.Text.RegularExpressions.RegexOptions.None);

        if (!game.Success) return delta;

        // Boyutu al
        if (!int.TryParse(game.Groups[1].Value, out var size)) return delta;

        // İçerik game'in bittiği yerden başlar
        var contentStart = game.Index + game.Length;

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