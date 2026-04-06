using HtmlAgilityPack;

namespace VolleyballScraper.Api.Services;

public class StandingsScraperService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<StandingsScraperService> _logger;

    private const string BaseUrl = $"{AppConstants.BaseUrl}/PuanDurumu";

    public StandingsScraperService(
        IHttpClientFactory httpClientFactory,
        ILogger<StandingsScraperService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches all competitions for the given season, category and league.
    /// Each competition corresponds to a phase (group stage, quarter-final, etc.).
    /// </summary>
    public async Task<List<Competition>> GetCompetitionsAsync(CompetitionRequest request)
    {
        var client = _httpClientFactory.CreateClient("VolleyballClient");

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

        return competitions;
    }

    /// <summary>
    /// Fetches the standings table and game results for a specific competition.
    /// </summary>
    public async Task<StandingsResponse> GetStandingsAsync(StandingsRequest request)
    {
        var client = _httpClientFactory.CreateClient("VolleyballClient");

        var (viewState, viewStateGen, cookie) = await GetViewStateAsync(client);
        _logger.LogInformation(
            "[Standings] VIEWSTATE retrieved for competition {id}.", request.CompetitionId);

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

        // Step 4: Select league
        (viewState, viewStateGen) = await PostStepAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$ddlskume",
            extraFields: BuildBaseFields(request.SeasonId, request.Category, request.LeagueCode));

        // Step 5: Select competition → triggers standings + game list render
        var rawValue = $"{request.CompetitionId}*{request.CompetitionKey}";
        var standingsRaw = await PostStepRawAsync(
            client, cookie, viewState, viewStateGen,
            eventTarget: "ctl00$icerik$GvTemplate_1$ctl04$lnkSubmit",
            extraFields: BuildCompetitionFields(
                request.SeasonId,
                request.Category,
                request.LeagueCode,
                request.CompetitionId,
                request.CompetitionKey));

        var html = ExtractUpdatePanelHtml(standingsRaw);

        var standings = ParseStandingsTable(html);
        var games = ParseGameResults(html);

        var hasGoztepe = standings.Any(r => r.IsGoztepe);

        _logger.LogInformation(
            "[Standings] Competition {id}: {standingsCount} teams, {gameCount} games, hasGöztepe={has}.",
            request.CompetitionId, standings.Count, games.Count, hasGoztepe);

        return new StandingsResponse
        {
            CompetitionId = request.CompetitionId,
            CompetitionName = "", // populated by controller from competition list
            SeasonId = request.SeasonId,
            HasGoztepe = hasGoztepe,
            Standings = standings,
            Games = games,
        };
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

        foreach (var option in options)
        {
            var rawValue = option.GetAttributeValue("value", "");
            var name = Clean(option.InnerText);

            // Skip the default "Seçiniz" placeholder option
            if (string.IsNullOrWhiteSpace(rawValue) || rawValue == "0") continue;

            // Raw value format: "19285*5DB65D03-09DD-4B8E-99E3-7940EEA1C725"
            var parts = rawValue.Split('*');
            if (parts.Length != 2) continue;

            competitions.Add(new Competition
            {
                Id = parts[0],
                Key = parts[1],
                Name = name,
                RawValue = rawValue,
                LeagueCode = request.LeagueCode,
                Category = request.Category,
                HasGoztepe = false, // resolved separately when standings are fetched
            });
        }

        return competitions;
    }

    /// <summary>
    /// Parses the standings table (puan durumu) from the rendered HTML.
    /// Expects a table with columns: rank, logo, team, O, G, M, A, V, P, SAV, ASP, VSP, SPAV, 3-0, 3-1, 3-2, 2-3, 1-3, 0-3
    /// </summary>
    private static List<StandingsRow> ParseStandingsTable(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // The standings table has a recognisable header; locate it by known column text
        var table = doc.DocumentNode
            .SelectNodes("//table")
            ?.FirstOrDefault(t => t.InnerHtml.Contains("SAV") && t.InnerHtml.Contains("SPAV"));

        if (table == null) return [];

        var rows = table.SelectNodes(".//tr")?.Skip(1).ToList();
        if (rows == null) return [];

        var standings = new List<StandingsRow>();
        var rank = 0;

        foreach (var row in rows)
        {
            var cols = row.SelectNodes(".//td");
            if (cols == null || cols.Count < 14) continue;

            // Column 0 may be rank number or logo — detect by checking if it is numeric
            var colOffset = int.TryParse(Clean(cols[0].InnerText), out _) ? 0 : 1;

            rank++;

            var teamName = Clean(cols[colOffset + 1].InnerText);
            if (string.IsNullOrWhiteSpace(teamName)) continue;

            standings.Add(new StandingsRow
            {
                Rank = rank,
                TeamName = teamName,
                Played = ParseInt(cols[colOffset + 2].InnerText),
                Won = ParseInt(cols[colOffset + 3].InnerText),
                Lost = ParseInt(cols[colOffset + 4].InnerText),
                SetsWon = ParseInt(cols[colOffset + 5].InnerText),
                SetsLost = ParseInt(cols[colOffset + 6].InnerText),
                Points = ParseInt(cols[colOffset + 7].InnerText),
                SetAverage = Clean(cols[colOffset + 8].InnerText),
                PointsWon = ParseInt(cols[colOffset + 9].InnerText),
                PointsLost = ParseInt(cols[colOffset + 10].InnerText),
                PointAverage = Clean(cols[colOffset + 11].InnerText),
                W30 = ParseInt(cols[colOffset + 12].InnerText),
                W31 = ParseInt(cols[colOffset + 13].InnerText),
                W32 = ParseInt(cols[colOffset + 14].InnerText),
                L23 = cols.Count > colOffset + 15 ? ParseInt(cols[colOffset + 15].InnerText) : 0,
                L13 = cols.Count > colOffset + 16 ? ParseInt(cols[colOffset + 16].InnerText) : 0,
                L03 = cols.Count > colOffset + 17 ? ParseInt(cols[colOffset + 17].InnerText) : 0,
                IsGoztepe = teamName.Contains(AppConstants.ClubName, StringComparison.OrdinalIgnoreCase),
            });
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

        // The game results table contains "Set Sonuçları" in its header
        var table = doc.DocumentNode
            .SelectNodes("//table")
            ?.FirstOrDefault(t => t.InnerHtml.Contains("Set Sonuçları"));

        if (table == null) return [];

        var rows = table.SelectNodes(".//tr")?.Skip(1).ToList();
        if (rows == null) return [];

        var results = new List<GameResult>();

        foreach (var row in rows)
        {
            var cols = row.SelectNodes(".//td");
            if (cols == null || cols.Count < 7) continue;

            var rowNoText = Clean(cols[0].InnerText);
            var homeTeam = Clean(cols[4].InnerText);
            var awayTeam = Clean(cols[6].InnerText);
            var setResults = cols.Count > 7 ? Clean(cols[7].InnerText) : "";

            // Parse set scores — shown as two adjacent cells with bold text when played
            var homeScoreText = cols.Count > 4 ? Clean(cols[5].InnerText) : "";
            var awayScoreText = cols.Count > 5 ? Clean(cols[5].InnerText) : "";

            // Score cells: col[5] = home sets, col[5] shared — detect from bold spans
            var scoreCells = row.SelectNodes(".//td[contains(@class,'score') or b]");
            var homeScore = 0;
            var awayScore = 0;
            var isPlayed = !string.IsNullOrWhiteSpace(setResults);

            if (isPlayed)
            {
                // Score format: two adjacent cells each containing a bold number
                var boldNodes = row.SelectNodes(".//b");
                if (boldNodes != null && boldNodes.Count >= 2)
                {
                    _ = int.TryParse(Clean(boldNodes[0].InnerText), out homeScore);
                    _ = int.TryParse(Clean(boldNodes[1].InnerText), out awayScore);
                }
            }

            results.Add(new GameResult
            {
                RowNo = ParseInt(rowNoText),
                Date = Clean(cols[1].InnerText),
                Time = Clean(cols[2].InnerText),
                Venue = Clean(cols[3].InnerText),
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                HomeScore = homeScore,
                AwayScore = awayScore,
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
        string competitionId, string competitionKey) =>
        new()
        {
            ["ctl00$icerik$ddlSil"] = AppConstants.ProvinceId,
            ["ctl00$icerik$ddlsbe"] = AppConstants.Gender,
            ["ctl00$icerik$ddlSkategori"] = category,
            ["ctl00$icerik$ddlskume"] = leagueCode,
            ["ctl00$icerik$ddlSyarismaadi"] = $"{competitionId}*{competitionKey}",
            ["ctl00$icerik$HfYYarismaid"] = competitionId,
            ["ctl00$icerik$HfYYarismakey"] = competitionKey,
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
}