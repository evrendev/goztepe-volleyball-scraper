using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using VolleyballScraper.Api.Controllers;
using VolleyballScraper.Api.Services;
using VolleyballScraper.Api.Models.Standings;

namespace VolleyballScraper.Tests.Controllers;

public class StandingsControllerTests
{
    private readonly Mock<IStandingsScraperService> _standingsServiceMock;
    private readonly Mock<IStandingsCacheService> _cacheServiceMock;
    private readonly StandingsController _controller;

    public StandingsControllerTests()
    {
        _standingsServiceMock = new Mock<IStandingsScraperService>();
        _cacheServiceMock = new Mock<IStandingsCacheService>();
        _controller = new StandingsController(_standingsServiceMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetCompetitions_ShouldReturnOkResult_WithValidRequest()
    {
        // Arrange
        var request = new CompetitionRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL"
        };

        var expectedCompetitions = new List<Competition>
        {
            new()
            {
                Name = "17345*8FF84B09-1B9E-4C7D-8BF7-F9EC67B4D5E2",
                DisplayName = "Süper Lig Genç Kızlar - B Gr",
                LeagueCode = "GKSL",
                Category = "GK",
                HasGoztepe = true
            }
        };

        _standingsServiceMock.Setup(x => x.GetCompetitionsAsync(It.IsAny<CompetitionRequest>()))
            .ReturnsAsync(expectedCompetitions);

        // Act
        var result = await _controller.GetCompetitions(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;

        var expectedResponse = new
        {
            seasonId = request.SeasonId,
            category = request.Category,
            leagueCode = request.LeagueCode,
            total = expectedCompetitions.Count,
            competitions = expectedCompetitions
        };

        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetCompetitions_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var request = new CompetitionRequest(); // Invalid request
        _controller.ModelState.AddModelError("SeasonId", "SeasonId is required");

        // Act
        var result = await _controller.GetCompetitions(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetStandings_ShouldReturnOkResult_WithValidRequest()
    {
        // Arrange
        var request = new StandingsRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL",
            CompetitionName = "17345*8FF84B09-1B9E-4C7D-8BF7-F9EC67B4D5E2"
        };

        var expectedResponse = new StandingsResponse
        {
            CompetitionName = "Süper Lig Genç Kızlar - B Gr",
            SeasonId = "2025-2026",
            HasGoztepe = true,
            Standings = new List<StandingsRow>
            {
                new()
                {
                    Rank = 1,
                    TeamName = "Göztepe",
                    Points = 14,
                    Played = 5,
                    Won = 5,
                    Lost = 0,
                    IsGoztepe = true
                }
            },
            Games = new List<GameResult>
            {
                new()
                {
                    RowNo = 1,
                    Date = "17.09.2025",
                    Time = "20:00",
                    HomeTeam = "Göztepe",
                    AwayTeam = "Test Team",
                    HomeScore = 3,
                    AwayScore = 1,
                    IsPlayed = true,
                    IsGoztepe = true
                }
            }
        };

        _standingsServiceMock.Setup(x => x.GetStandingsAsync(It.IsAny<StandingsRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetStandings(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Theory]
    [InlineData("", "GK", "GKSL", "comp")]
    [InlineData("2025", "", "GKSL", "comp")]
    [InlineData("2025", "GK", "", "comp")]
    [InlineData("2025", "GK", "GKSL", "")]
    public async Task GetStandings_ShouldReturnBadRequest_WhenRequiredFieldsMissing(
        string seasonId, string category, string leagueCode, string competitionName)
    {
        // Arrange
        var request = new StandingsRequest
        {
            SeasonId = seasonId,
            Category = category,
            LeagueCode = leagueCode,
            CompetitionName = competitionName
        };

        // Simulate model validation failure
        if (string.IsNullOrEmpty(seasonId))
            _controller.ModelState.AddModelError("SeasonId", "SeasonId is required");
        if (string.IsNullOrEmpty(category))
            _controller.ModelState.AddModelError("Category", "Category is required");
        if (string.IsNullOrEmpty(leagueCode))
            _controller.ModelState.AddModelError("LeagueCode", "LeagueCode is required");
        if (string.IsNullOrEmpty(competitionName))
            _controller.ModelState.AddModelError("CompetitionName", "CompetitionName is required");

        // Act
        var result = await _controller.GetStandings(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetStandings_ShouldReturnInternalServerError_WhenServiceThrows()
    {
        // Arrange
        var request = new StandingsRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL",
            CompetitionName = "test"
        };

        _standingsServiceMock.Setup(x => x.GetStandingsAsync(It.IsAny<StandingsRequest>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetStandings(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}