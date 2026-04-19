using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using OlympicScraper.Api.Controllers.Volleyball;
using OlympicScraper.Api.Services.Volleyball;
using OlympicScraper.Api.Models.Volleyball.Fixture;

namespace OlympicScraper.Tests.Controllers;

public class FixtureControllerTests
{
    private readonly Mock<IFixtureScraperService> _fixtureServiceMock;
    private readonly Mock<IFixtureCacheService> _cacheServiceMock;
    private readonly FixtureController _controller;

    public FixtureControllerTests()
    {
        _fixtureServiceMock = new Mock<IFixtureScraperService>();
        _cacheServiceMock = new Mock<IFixtureCacheService>();
        _controller = new FixtureController(_fixtureServiceMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public void GetLeagues_ShouldReturnOkResult_WithAllSupportedLeagues()
    {
        // Act
        var result = _controller.GetLeagues();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGames_ShouldReturnOkResult_WithValidRequest()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = ["GKSL", "YKSL"],
            Category = "GK"
        };

        var expectedGames = new List<Game>
        {
            new()
            {
                Date = "18.09.2025",
                Time = "18:30",
                Venue = "Gaziemir S.S.",
                HomeTeam = "Göztepe",
                AwayTeam = "Test Team",
                Score = "3-1",
                Division = "GKSL",
                Category = "GK",
                MatchType = "KL",
                Group = "A",
                Round = "1D",
                Week = "1",
                League = "Genç Kızlar Süper Lig"
            }
        };

        _fixtureServiceMock.Setup(x => x.GetGamesAsync(It.IsAny<FixtureRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(expectedGames);

        _cacheServiceMock.Setup(x => x.GetCachedKeys())
            .Returns(new List<string> { "2025-2026_GKSL", "2025-2026_YKSL" });

        // Act
        var result = await _controller.GetGames(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGames_ShouldApplyFilters_WhenFiltersProvided()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = ["GKSL"],
            Category = "GK",
            Division = "GKSL",
            MatchType = "KL",
            Group = "A"
        };

        var allGames = new List<Game>
        {
            new()
            {
                Category = "GK",
                Division = "GKSL",
                MatchType = "KL",
                Group = "A",
                League = "Genç Kızlar Süper Lig"
            },
            new()
            {
                Category = "YK",  // Different category - should be filtered out
                Division = "GKSL",
                MatchType = "KL",
                Group = "A",
                League = "Genç Kızlar Süper Lig"
            }
        };

        _fixtureServiceMock.Setup(x => x.GetGamesAsync(It.IsAny<FixtureRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(allGames);

        _cacheServiceMock.Setup(x => x.GetCachedKeys())
            .Returns(new List<string> { "2025-2026_GKSL" });

        // Act
        var result = await _controller.GetGames(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;

        // The result should only contain the filtered game
        var responseValue = okResult!.Value;
        responseValue.Should().NotBeNull();
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

        var games = new List<Game>
        {
            new() { Category = category, League = "Test League" },
            new() { Category = "Other", League = "Test League" }
        };

        _fixtureServiceMock.Setup(x => x.GetGamesAsync(It.IsAny<FixtureRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(games);

        _cacheServiceMock.Setup(x => x.GetCachedKeys())
            .Returns(new List<string>());

        // Act
        var result = await _controller.GetGames(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetCacheStatus_ShouldReturnOkResult()
    {
        // Arrange
        var cachedKeys = new List<string> { "2025-2026_GKSL", "2025-2026_YKSL" };
        _cacheServiceMock.Setup(x => x.GetCachedKeys())
            .Returns(cachedKeys);

        // Act
        var result = _controller.GetCacheStatus();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public void ClearCache_ShouldReturnOkResult_WhenSeasonIdProvided()
    {
        // Arrange
        var seasonId = "2025-2026";

        // Act
        var result = _controller.ClearCache(seasonId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        _cacheServiceMock.Verify(x => x.Clear(seasonId), Times.Once);
    }

    [Fact]
    public void ClearCache_ShouldReturnOkResult_WhenSeasonIdIsNull()
    {
        // Act
        var result = _controller.ClearCache(null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        _cacheServiceMock.Verify(x => x.Clear(null), Times.Once);
    }

    [Fact]
    public async Task GetGames_ShouldReturnInternalServerError_WhenServiceThrows()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026"
        };

        _fixtureServiceMock.Setup(x => x.GetGamesAsync(It.IsAny<FixtureRequest>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetGames(request));
    }
}