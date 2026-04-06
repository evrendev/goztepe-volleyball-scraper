namespace VolleyballScraper.Api.Constants;

/// <summary>Club and federation constants — single source of truth.</summary>
public static class AppConstants
{
    /// <summary>Göztepe Sports Club organisation ID on the federation site.</summary>
    public const string OrganisationId = "662";

    /// <summary>Gender filter: B = Bayan (Women), E = Erkek (Men).</summary>
    public const string Gender = "B";

    /// <summary>Province ID for İzmir on the federation site.</summary>
    public const string ProvinceId = "35";

    /// <summary>
    /// The club name as it appears in fixture home/away team fields.
    /// Used for filtering — partial match applied (e.g. "Göztepe - A" also matches).
    /// </summary>
    public const string ClubName = "Göztepe";

    /// <summary>Default season ID if none is specified.</summary>
    public const string SeasonId = "2025-2026";

    /// <summary>
    /// Cache duration in hours for fixture data. Adjust based on how often the federation site updates.
    /// </summary>
    public const int FixtureCacheDuration = 24;

    /// <summary>
    /// Cache duration in hours for standings data. Longer duration since standings change less frequently.
    /// Adjust based on how often the federation site updates standings.
    /// </summary>
    public const int StandingsCacheDuration = 720; // 30 days

    /// <summary>
    /// Timeout duration in seconds for HTTP requests. Adjust based on network conditions and federation site responsiveness.
    /// </summary>
    public const int Timeout = 60;

    /// <summary>
    /// Base URL of the İzmir Volleyball Federation website. Used for constructing HTTP client and Referer header.
    /// </summary>
    public const string BaseUrl = "https://izmir.voleyboliltemsilciligi.com";
}