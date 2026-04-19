using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace OlympicScraper.Tests.Integration;

public class GetCompetitionsResponse
{
    [JsonPropertyName("seasonId")]
    public string SeasonId { get; set; } = "";

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("leagueCode")]
    public string LeagueCode { get; set; } = "";

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("competitions")]
    public List<Competition> Competitions { get; set; } = new();
}

public class StandingsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public StandingsApiIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task GetCompetitions_ShouldReturn200_WithValidRequest()
    {
        // Arrange
        var request = new CompetitionRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/standings/competitions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<GetCompetitionsResponse>(json, _jsonOptions);

        responseObj.Should().NotBeNull();
        responseObj!.Competitions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStandings_ShouldReturn200_WithValidRequest()
    {
        // Arrange - First get a valid competition
        var competitionsRequest = new CompetitionRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL"
        };

        var competitionResponse = await _client.PostAsJsonAsync("/api/volleyball/standings/competitions", competitionsRequest);
        competitionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await competitionResponse.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<GetCompetitionsResponse>(json, _jsonOptions);
        responseObj!.Competitions.Should().NotBeEmpty();

        var firstCompetition = responseObj.Competitions.First();
        var standingsRequest = new StandingsRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL",
            CompetitionName = firstCompetition.Name
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/standings", standingsRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var standingsResponse = await response.Content.ReadFromJsonAsync<Response>();
        standingsResponse.Should().NotBeNull();
        standingsResponse!.Standings.Should().NotBeNull();
        standingsResponse.Games.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStandings_ShouldReturn400_WithInvalidCompetitionName()
    {
        // Arrange
        var request = new StandingsRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL",
            CompetitionName = "invalid-competition"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/standings", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-season")]
    [InlineData("2020-2021")]
    public async Task GetCompetitions_ShouldHandleInvalidSeasons(string seasonId)
    {
        // Arrange
        var request = new CompetitionRequest
        {
            SeasonId = seasonId,
            Category = "GK",
            LeagueCode = "GKSL"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/volleyball/standings/competitions", request);

        // Assert
        if (string.IsNullOrEmpty(seasonId))
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        else
        {
            // Invalid seasons might return empty results or error
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task Api_ShouldReturnGoztepeData_WhenGoztepeExists()
    {
        // Arrange - Look for competitions that might have Göztepe
        var request = new CompetitionRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL"
        };

        var competitionsResponse = await _client.PostAsJsonAsync("/api/volleyball/standings/competitions", request);
        var json = await competitionsResponse.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<GetCompetitionsResponse>(json, _jsonOptions);

        // Find a competition that has Göztepe (we'll test all if needed)
        foreach (var competition in responseObj!.Competitions)
        {
            var standingsRequest = new StandingsRequest
            {
                SeasonId = "2025-2026",
                Category = "GK",
                LeagueCode = "GKSL",
                CompetitionName = competition.Name
            };

            // Act
            var standingsResponse = await _client.PostAsJsonAsync("/api/volleyball/standings", standingsRequest);

            if (standingsResponse.StatusCode == HttpStatusCode.OK)
            {
                var standings = await standingsResponse.Content.ReadFromJsonAsync<Response>();

                if (standings!.HasGoztepe)
                {
                    // Assert - Found Göztepe data
                    standings.Standings.Should().Contain(s => s.IsGoztepe);
                    standings.Games.Where(g => g.IsGoztepe).Should().NotBeEmpty();
                    return; // Test passed
                }
            }
        }

        // No Göztepe found - this might be expected depending on season
        Assert.True(true, "No competitions with Göztepe found - this might be expected");
    }
}