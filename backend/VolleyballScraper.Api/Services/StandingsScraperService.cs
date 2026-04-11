using HtmlAgilityPack;

namespace VolleyballScraper.Api.Services;

public class StandingsScraperService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<StandingsScraperService> _logger;
    private readonly StandingsCacheService _cache;
    private const string BaseUrl = $"{AppConstants.BaseUrl}/PuanDurumu";

    public StandingsScraperService(
        IHttpClientFactory httpClientFactory,
        ILogger<StandingsScraperService> logger,
        StandingsCacheService cache)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _cache = cache;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches all competitions for the given season, category and league.
    /// Each competition corresponds to a phase (group stage, quarter-final, etc.).
    /// </summary>
    public async Task<List<Competition>> GetCompetitionsAsync(CompetitionRequest request)
    {
        var cacheKey = _cache.BuildCompetitionsKey(
            request.SeasonId, request.Category, request.LeagueCode);

        if (_cache.TryGetCompetitions(cacheKey, out var cached))
            return cached;

        var client = _httpClientFactory.CreateClient("StandingsClient");

        var (viewState, viewStateGen, cookie) = await GetViewStateAsync(client);
        _logger.LogInformation("[Standings] VIEWSTATE retrieved for competitions.");

        // Step 1: Select season
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlSil",
            extraFields: BuildBaseFields(request.SeasonId, "0", "0"));

        // Step 2: Select gender → populates category dropdown
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlsbe",
            extraFields: BuildBaseFields(request.SeasonId, "0", "0"));

        // Step 3: Select category → populates league dropdown
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlSkategori",
            extraFields: BuildBaseFields(request.SeasonId, request.Category, "0"));

        // Step 4: Select league → populates competition (yarışma adı) dropdown
        var leagueRaw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskume",
            extraFields: BuildBaseFields(request.SeasonId, request.Category, request.LeagueCode));

        var competitions = ParseCompetitionDropdown(leagueRaw, request);

        _logger.LogInformation(
            "[Standings] Found {count} competitions for {league}.",
            competitions.Count, request.LeagueCode);

        _cache.SetCompetitions(cacheKey, competitions);
        return competitions;
    }

    /// <summary>
    /// Fetches the standings table and game results for a specific competition.
    /// </summary>
    public async Task<StandingsResponse> GetStandingsAsync(StandingsRequest request)
    {
        var cacheKey = _cache.BuildStandingsKey(
            request.SeasonId, request.CompetitionName);

        if (_cache.TryGetStandings(cacheKey, out var cached))
            return cached;

        var client = _httpClientFactory.CreateClient("StandingsClient");

        var (viewState, viewStateGen, cookie) = await GetViewStateAsync(client);
        _logger.LogInformation(
            "[Standings] VIEWSTATE retrieved for competition {name}.", request.CompetitionName);

        // Step 1: Select season
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlSil",
            extraFields: BuildBaseFields(request.SeasonId, "0", "0"));

        // Step 2: Select gender
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlsbe",
            extraFields: BuildBaseFields(request.SeasonId, "0", "0"));

        // Step 3: Select category
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlSkategori",
            extraFields: BuildBaseFields(request.SeasonId, request.Category, "0"));

        var step4Raw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskume",
            extraFields: BuildBaseFields(request.SeasonId, request.Category, request.LeagueCode));

        // Extract competition name from dropdown before proceeding
        var competitionName = ExtractCompetitionName(step4Raw, request.CompetitionName);
        (viewState, viewStateGen) = ExtractViewState(step4Raw, viewState, viewStateGen);

        // Step 5: Select competition
        var standingsRaw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$GvTemplate_1$ctl04$lnkSubmit",
            extraFields: BuildCompetitionFields(
                request.SeasonId,
                request.Category,
                request.LeagueCode,
                request.CompetitionName));

        var html = ExtractUpdatePanelHtml(standingsRaw);

        var standings = ParseStandingsTable(html);
        var games = ParseGameResults(html);

        var hasGoztepe = standings.Any(r => r.IsGoztepe);

        _logger.LogInformation(
            "[Standings] Competition {name}: {standingsCount} teams, {gameCount} games, hasGöztepe={has}.",
            request.CompetitionName, standings.Count, games.Count, hasGoztepe);

        var response = new StandingsResponse
        {
            CompetitionName = competitionName,
            SeasonId = request.SeasonId,
            HasGoztepe = hasGoztepe,
            Standings = standings,
            Games = games,
        };

        _cache.SetStandings(cacheKey, response);
        return response;
    }

    // ── Parse ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses the competition dropdown (yarışma adı) from the raw delta response.
    /// Each option has value format "id*key" and text is the competition name.
    /// </summary>
    private static List<Competition> ParseCompetitionDropdown(
        string rawResponse, CompetitionRequest request)
    {
        var html = ExtractUpdatePanelHtml(rawResponse);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var select = doc.DocumentNode
            .SelectSingleNode("//select[@id='icerik_ddlSyarismaadi']");

        if (select == null) return [];

        var competitions = new List<Competition>();
        var options = select.SelectNodes(".//option");
        if (options == null || options.Count < 2) return competitions;

        foreach (var option in options.Skip(1)) // skip the first "Seçiniz" option
        {
            var name = option.GetAttributeValue("value", "");
            var title = System.Net.WebUtility.HtmlDecode(option.InnerText.Trim());

            // Skip the default "Seçiniz" placeholder option
            if (string.IsNullOrWhiteSpace(name)) continue;

            competitions.Add(new Competition
            {
                Name = name,
                DisplayName = title,
                LeagueCode = request.LeagueCode,
                Category = request.Category,
                HasGoztepe = false, // resolved separately when standings are fetched
            });
        }

        return competitions;
    }

    /// <summary>
    /// Parses the standings table using span element IDs which follow a
    /// predictable pattern: icerik_GvTemplate_1_{fieldCode}_{rowIndex}
    /// This is more reliable than td index-based parsing.
    /// </summary>
    private static List<StandingsRow> ParseStandingsTable(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var standings = new List<StandingsRow>();
        var rowIndex = 0;

        while (true)
        {
            // Team name is inside an <a> tag — its id follows: GvTemplate_1_lnkSubmit_{index}
            // We skip index 0 which is the header submit link, data rows start at ctl02 → index 0
            var teamLink = doc.GetElementbyId($"icerik_GvTemplate_1_lnkSubmit_{rowIndex}");
            if (teamLink == null) break; // no more rows

            var teamName = Clean(teamLink.InnerText);

            // Logo img element id: icerik_GvTemplate_1_imgFoto_{index}
            var logoNode = doc.GetElementbyId($"icerik_GvTemplate_1_imgFoto_{rowIndex}");
            var logoSrc = logoNode?.GetAttributeValue("src", "") ?? "";

            // Strip cache-busting query string and build absolute URL
            if (!string.IsNullOrWhiteSpace(logoSrc))
            {
                var cleanSrc = logoSrc.Split('?')[0];
                logoSrc = $"https://izmir.voleyboliltemsilciligi.com/{cleanSrc.TrimStart('/')}";
            }

            standings.Add(new StandingsRow
            {
                Rank = rowIndex + 1,
                TeamName = teamName,
                LogoUrl = logoSrc,
                Played = GetSpanInt(doc, $"icerik_GvTemplate_1_gO_{rowIndex}"),
                Won = GetSpanInt(doc, $"icerik_GvTemplate_1_gG_{rowIndex}"),
                Lost = GetSpanInt(doc, $"icerik_GvTemplate_1_gM_{rowIndex}"),
                SetsWon = GetSpanInt(doc, $"icerik_GvTemplate_1_gA_{rowIndex}"),
                SetsLost = GetSpanInt(doc, $"icerik_GvTemplate_1_gV_{rowIndex}"),
                Points = GetSpanInt(doc, $"icerik_GvTemplate_1_gP_{rowIndex}"),
                SetAverage = GetSpanText(doc, $"icerik_GvTemplate_1_gSAV_{rowIndex}"),
                PointsWon = GetSpanInt(doc, $"icerik_GvTemplate_1_gASP_{rowIndex}"),
                PointsLost = GetSpanInt(doc, $"icerik_GvTemplate_1_gVSP_{rowIndex}"),
                PointAverage = GetSpanInt(doc, $"icerik_GvTemplate_1_gSPAV_{rowIndex}"),
                W30 = GetSpanInt(doc, $"icerik_GvTemplate_1_gA3_0_{rowIndex}"),
                W31 = GetSpanInt(doc, $"icerik_GvTemplate_1_gA3_1_{rowIndex}"),
                W32 = GetSpanInt(doc, $"icerik_GvTemplate_1_gA3_2_{rowIndex}"),
                L23 = GetSpanInt(doc, $"icerik_GvTemplate_1_gV2_3_{rowIndex}"),
                L13 = GetSpanInt(doc, $"icerik_GvTemplate_1_gV1_3_{rowIndex}"),
                L03 = GetSpanInt(doc, $"icerik_GvTemplate_1_gV0_3_{rowIndex}"),
                IsGoztepe = teamName.Contains(
                    AppConstants.ClubName, StringComparison.OrdinalIgnoreCase),
            });

            rowIndex++;
        }

        return standings;
    }

    /// <summary>
    /// Parses the game result list shown below the standings table.
    /// Columns: S.No, Tarih, Saat, Salon Adı, Ev Sahibi (A), score, Misafir (B), Set Sonuçları
    /// </summary>
    private static List<GameResult> ParseGameResults(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Match results table uses a different template — find by "Set Sonuçları" header
        var table = doc.DocumentNode
            .SelectSingleNode("//table[@id='icerik_gvmusabakaliste']");

        if (table == null) return [];

        var rows = table.SelectNodes(".//tr")?.Skip(1).ToList();
        if (rows == null) return [];

        var results = new List<GameResult>();
        var lastDate = "";
        var lastTime = "";
        var lastVenue = "";

        foreach (var row in rows)
        {
            var cols = row.SelectNodes(".//td");
            if (cols == null || cols.Count < 6) continue;

            // Col 0 = S.No, 1 = Tarih, 2 = Saat, 3 = Salon, 4 = Ev Sahibi
            // Col 5 = home score (bold), 6 = away score (bold), 7 = Misafir, 8 = Set Sonuçları
            var rowNoText = Clean(cols[0].InnerText);
            if (!int.TryParse(rowNoText, out var rowNo)) continue;

            var rawDate = Clean(cols[1].InnerText);
            var rawTime = Clean(cols[2].InnerText);
            var rawVenue = Clean(cols[3].InnerText);

            // Carry-forward date/time/venue for grouped rows
            if (!string.IsNullOrWhiteSpace(rawDate)) { lastDate = rawDate; lastTime = rawTime; lastVenue = rawVenue; }

            var homeTeam = cols.Count > 4 ? Clean(cols[4].InnerText) : "";
            var homeScore = cols.Count > 5 ? Clean(cols[5].InnerText) : "";
            var awayScore = cols.Count > 6 ? Clean(cols[6].InnerText) : "";
            var awayTeam = cols.Count > 7 ? Clean(cols[7].InnerText) : "";
            var setResults = cols.Count > 8 ? Clean(cols[8].InnerText) : "";

            var isPlayed = int.TryParse(homeScore, out var hs) &
               int.TryParse(awayScore, out var as_) &
               (hs > 0 || as_ > 0);

            results.Add(new GameResult
            {
                RowNo = rowNo,
                Date = string.IsNullOrWhiteSpace(rawDate) ? lastDate : rawDate,
                Time = string.IsNullOrWhiteSpace(rawTime) ? lastTime : rawTime,
                Venue = string.IsNullOrWhiteSpace(rawVenue) ? lastVenue : rawVenue,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                HomeScore = isPlayed ? hs : 0,
                AwayScore = isPlayed ? as_ : 0,
                SetResults = setResults,
                IsPlayed = isPlayed,
                IsGoztepe =
                    homeTeam.Contains(AppConstants.ClubName, StringComparison.OrdinalIgnoreCase) ||
                    awayTeam.Contains(AppConstants.ClubName, StringComparison.OrdinalIgnoreCase),
            });
        }

        return results;
    }

    // ── HTTP helpers ─────────────────────────────────────────────────────────

    private async Task<(string viewState, string viewStateGen, string cookie)>
        GetViewStateAsync(HttpClient client)
    {
        var response = await client.GetAsync(BaseUrl);
        var html = await response.Content.ReadAsStringAsync();

        // Collect session cookie from Set-Cookie headers
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

    private async Task<(string viewState, string viewStateGen)> PostStepAsync(
        HttpClient client, string cookie,
        string viewState, string viewStateGen,
        string eventTarget,
        Dictionary<string, string> extraFields)
    {
        var raw = await PostStepRawAsync(client, cookie, viewState, viewStateGen, eventTarget, extraFields);
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
        string eventTarget,
        Dictionary<string, string> extraFields)
    {
        var formData = new Dictionary<string, string>
        {
            ["ctl00$ScriptManager1"] = $"ctl00$icerik$UpdatePanel|{eventTarget}",
            ["ctl00$mail"] = "",
            ["ctl00$password"] = "",
            ["ctl00$icerik$ddlSil"] = AppConstants.ProvinceId,
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

    // ── Field builders ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds the standard form fields shared across all steps.
    /// </summary>
    private static Dictionary<string, string> BuildBaseFields(
        string seasonId, string category, string leagueCode) =>
        new()
        {
            ["ctl00$icerik$ddlSil"] = AppConstants.ProvinceId,
            ["ctl00$icerik$ddlsbe"] = AppConstants.Gender,
            ["ctl00$icerik$ddlSkategori"] = category,
            ["ctl00$icerik$ddlskume"] = leagueCode,
            ["ctl00$icerik$ddlSyarismaadi"] = "0",
        };

    /// <summary>
    /// Builds the extended form fields required for the competition selection step,
    /// including the hidden fields that the site uses to identify the competition.
    /// </summary>
    private static Dictionary<string, string> BuildCompetitionFields(
        string seasonId, string category, string leagueCode,
        string competitionName) =>
        new()
        {
            ["ctl00$icerik$ddlSil"] = AppConstants.ProvinceId,
            ["ctl00$icerik$ddlsbe"] = AppConstants.Gender,
            ["ctl00$icerik$ddlSkategori"] = category,
            ["ctl00$icerik$ddlskume"] = leagueCode,
            ["ctl00$icerik$ddlSyarismaadi"] = competitionName,
            ["ctl00$icerik$HfYKategoriid"] = category,
            ["ctl00$icerik$HfYTurid"] = "",  // populated dynamically by site JS
            ["ctl00$icerik$HfYGrupid"] = "",  // populated dynamically by site JS
            ["ctl00$icerik$HfYSiteid"] = AppConstants.ProvinceId,
            ["ctl00$icerik$HfYTipid"] = "A", // A = standings + games view
            ["ctl00$icerik$HfYMacTemplate"] = "1",
        };

    // ── Delta response helpers ───────────────────────────────────────────────

    private static string ExtractUpdatePanelHtml(string delta)
    {
        // Delta format: "length|updatePanel|panelId|html|..."
        var match = System.Text.RegularExpressions.Regex.Match(
            delta, @"\d+\|updatePanel\|[^|]+\|");
        if (!match.Success) return delta;

        var start = match.Index + match.Length;
        var len = int.Parse(match.Value.Split('|')[0]);
        return start + len <= delta.Length
            ? delta.Substring(start, len)
            : delta[start..];
    }

    private static string ExtractHiddenField(string delta, string fieldName)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            delta, $@"\d+\|hiddenField\|{fieldName}\|([^|]*)");
        return match.Success ? match.Groups[1].Value : "";
    }

    // ── String helpers ───────────────────────────────────────────────────────

    private static string Clean(string input) =>
        System.Net.WebUtility.HtmlDecode(input).Trim();

    private static int ParseInt(string input)
    {
        var clean = Clean(input).Replace(".", "").Replace(",", "");
        return int.TryParse(clean, out var result) ? result : 0;
    }

    // ── Span ID helpers ──────────────────────────────────────────────────────────

    private static string GetSpanText(HtmlDocument doc, string spanId)
    {
        var node = doc.GetElementbyId(spanId);
        return node == null ? "" : Clean(node.InnerText);
    }

    private static int GetSpanInt(HtmlDocument doc, string spanId)
    {
        var text = GetSpanText(doc, spanId);
        return int.TryParse(text.Replace(".", "").Replace(",", ""), out var val) ? val : 0;
    }

    /// <summary>
    /// Extracts the competition display name from the yarışma adı dropdown
    /// by matching the option value that starts with the given competition name.
    /// </summary>
    private static string ExtractCompetitionName(string rawResponse, string competitionName)
    {
        var html = ExtractUpdatePanelHtml(rawResponse);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var select = doc.DocumentNode
            .SelectSingleNode("//select[@id='icerik_ddlSyarismaadi']");

        if (select == null) return "";

        var option = select
            .SelectNodes(".//option")
            ?.FirstOrDefault(o =>
                o.GetAttributeValue("value", "").StartsWith($"{competitionName}*"));

        return option == null ? "" : Clean(option.InnerText);
    }

    /// <summary>
    /// Extracts updated VIEWSTATE values from a delta response,
    /// falling back to the previous values if not present.
    /// </summary>
    private static (string viewState, string viewStateGen) ExtractViewState(
        string raw, string currentViewState, string currentViewStateGen)
    {
        var newVs = ExtractHiddenField(raw, "__VIEWSTATE");
        var newGen = ExtractHiddenField(raw, "__VIEWSTATEGENERATOR");

        return (
            string.IsNullOrEmpty(newVs) ? currentViewState : newVs,
            string.IsNullOrEmpty(newGen) ? currentViewStateGen : newGen
        );
    }
}