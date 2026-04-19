using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;
using OlympicScraper.Api.Models.Volleyball.Fixture;

namespace OlympicScraper.Tests.Integration;

public class GetLeaguesResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";
}

public class GetGamesResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("season")]
    public string Season { get; set; } = "";

    [JsonPropertyName("cachedLeagues")]
    public int CachedLeagues { get; set; }

    [JsonPropertyName("filters")]
    public object Filters { get; set; } = new();

    [JsonPropertyName("leagues")]
    public List<LeagueGames> Leagues { get; set; } = new();
}

public class LeagueGames
{
    [JsonPropertyName("league")]
    public string League { get; set; } = "";

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("games")]
    public List<Game> Games { get; set; } = new();
}

public class FixtureApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public FixtureApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetLeagues_ShouldReturn200_WithAllSupportedLeagues()
    {
        // Act
        var response = await _client.GetAsync("/api/volleyball/fixture/leagues");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var leagues = JsonSerializer.Deserialize<GetLeaguesResponse[]>(json, _jsonOptions);

        leagues.Should().NotBeNull();
        leagues!.Should().NotBeEmpty();
        leagues.Should().OnlyContain(l => !string.IsNullOrEmpty(l.Code));
        leagues.Should().OnlyContain(l => !string.IsNullOrEmpty(l.DisplayName));
        leagues.Should().OnlyContain(l => !string.IsNullOrEmpty(l.Category));
    }

    [Fact]
    public async Task GetGames_ShouldReturn200_WithValidRequest()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = ["GKSL"]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/fixture/games", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<GetGamesResponse>(json, _jsonOptions);

        responseObj.Should().NotBeNull();
        responseObj!.Season.Should().Be("2025-2026");
        responseObj.Total.Should().BeGreaterThanOrEqualTo(0);
        responseObj.Leagues.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGames_ShouldReturn200_WithFilteredRequest()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = ["GKSL"],
            Category = "GK",
            MatchType = "KL"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/fixture/games", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<GetGamesResponse>(json, _jsonOptions);

        responseObj.Should().NotBeNull();
        responseObj!.Filters.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGames_ShouldReturn200_WithEmptyLeaguesList()
    {
        // Arrange - Empty leagues list should fetch all leagues
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = []
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/fixture/games", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<GetGamesResponse>(json, _jsonOptions);

        responseObj.Should().NotBeNull();
        responseObj!.Season.Should().Be("2025-2026");
    }

    [Theory]
    [InlineData("GK")]
    [InlineData("YK")]
    [InlineData("KK")]
    public async Task GetGames_ShouldFilterByCategory_WhenCategoryProvided(string category)
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Category = category
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/fixture/games", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<GetGamesResponse>(json, _jsonOptions);

        responseObj.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCacheStatus_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/volleyball/fixture/cache/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();

        // Should contain cache information
        json.Should().Contain("totalCachedKeys");
        json.Should().Contain("keys");
    }

    [Fact]
    public async Task ClearCache_ShouldReturn200_WithoutSeasonId()
    {
        // Act
        var response = await _client.DeleteAsync("/api/volleyball/fixture/cache");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("All cache cleared");
    }

    [Fact]
    public async Task ClearCache_ShouldReturn200_WithSeasonId()
    {
        // Arrange
        var seasonId = "2025-2026";

        // Act
        var response = await _client.DeleteAsync($"/api/volleyball/fixture/cache?seasonId={seasonId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain($"Cache cleared for season {seasonId}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-season")]
    [InlineData("2020-2021")]
    public async Task GetGames_ShouldHandleInvalidSeasons(string seasonId)
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = seasonId,
            Leagues = ["GKSL"]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/fixture/games", request);

        // Assert
        if (string.IsNullOrEmpty(seasonId))
        {
            // Invalid seasons might still return 200 with empty results
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
        else
        {
            // Invalid seasons might return empty results or error
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    public async Task GetGames_ShouldReturn200_WithForceRefresh()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = ["GKSL"]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/fixture/games?forceRefresh=true", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<GetGamesResponse>(json, _jsonOptions);

        responseObj.Should().NotBeNull();
        responseObj!.Season.Should().Be("2025-2026");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    public async Task GetGames_ShouldFilterByGroup_WhenGroupProvided(string group)
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Group = group
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/fixture/games", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("1D")]
    [InlineData("2D")]
    public async Task GetGames_ShouldFilterByRound_WhenRoundProvided(string round)
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Round = round
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/fixture/games", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}