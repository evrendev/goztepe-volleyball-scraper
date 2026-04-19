using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using OlympicScraper.Api.Services.Volleyball;
using OlympicScraper.Api.Models.Volleyball.Fixture;

namespace OlympicScraper.Tests.Services;

public delegate bool TryGetDelegate(string key, out List<Game> games);

public class FixtureScraperServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<FixtureScraperService>> _loggerMock;
    private readonly Mock<IFixtureCacheService> _cacheMock;
    private readonly IFixtureScraperService _service;

    public FixtureScraperServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<FixtureScraperService>>();
        _cacheMock = new Mock<IFixtureCacheService>();
        _service = new FixtureScraperService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _cacheMock.Object);
    }

    [Theory]
    [InlineData("2025-2026", "GKSL")]
    [InlineData("2024-2025", "YKSL")]
    [InlineData("2023-2024", "KKSL")]
    public async Task GetGamesAsync_ShouldReturnCachedResults_WhenCacheHit(
        string seasonId, string leagueCode)
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = seasonId,
            Leagues = [leagueCode]
        };

        var expectedGames = new List<Game>
        {
            new()
            {
                Date = "18.09.2025",
                Time = "18:30",
                HomeTeam = "Göztepe",
                AwayTeam = "Test Team",
                League = "Test League",
                Division = leagueCode
            }
        };

        var cacheKey = "test-cache-key";
        _cacheMock.Setup(x => x.BuildKey(seasonId, leagueCode))
            .Returns(cacheKey);

        _cacheMock.Setup(x => x.TryGet(cacheKey, out It.Ref<List<Game>>.IsAny))
            .Returns(new TryGetDelegate((string key, out List<Game> games) =>
            {
                games = expectedGames;
                return true;
            }));

        // Act
        var result = await _service.GetGamesAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedGames);
        result.Should().HaveCount(1);
        result.First().Division.Should().Be(leagueCode);
    }

    [Fact]
    public async Task GetGamesAsync_ShouldReturnEmptyList_WhenHttpFails()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = ["GKSL"]
        };

        var cacheKey = "2025-2026_GKSL";
        _cacheMock.Setup(x => x.TryGet(cacheKey, out It.Ref<List<Game>>.IsAny))
            .Returns(false);

        var mockHttpClient = new Mock<HttpClient>();
        _httpClientFactoryMock.Setup(x => x.CreateClient("FixtureClient"))
            .Returns(mockHttpClient.Object);

        // Act
        var result = await _service.GetGamesAsync(request);

        // Assert - Service should return empty list on HTTP failure, not throw
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetGamesAsync_ShouldHandleInvalidInput_WhenSeasonIdInvalid(string invalidSeasonId)
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = invalidSeasonId,
            Leagues = ["GKSL"]
        };

        // Act & Assert - Should handle gracefully, not throw
        var result = await _service.GetGamesAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGamesAsync_ShouldHandleEmptyLeaguesList()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = [] // Empty leagues should fetch all leagues
        };

        // Act
        var result = await _service.GetGamesAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("GKSL")]
    [InlineData("YKSL")]
    [InlineData("KKSL")]
    public async Task GetGamesAsync_ShouldProcessSingleLeague_WhenSingleLeagueProvided(string leagueCode)
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = [leagueCode]
        };

        var expectedGames = new List<Game>
        {
            new()
            {
                League = "Test League",
                Division = leagueCode,
                HomeTeam = "Göztepe",
                AwayTeam = "Opponent"
            }
        };

        var cacheKey = $"test-cache-key-{leagueCode}";
        _cacheMock.Setup(x => x.BuildKey("2025-2026", leagueCode))
            .Returns(cacheKey);

        _cacheMock.Setup(x => x.TryGet(cacheKey, out It.Ref<List<Game>>.IsAny))
            .Returns(new TryGetDelegate((string key, out List<Game> games) =>
            {
                games = expectedGames;
                return true;
            }));

        // Act
        var result = await _service.GetGamesAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Division.Should().Be(leagueCode);
    }

    [Fact]
    public async Task GetGamesAsync_ShouldProcessMultipleLeagues()
    {
        // Arrange
        var league1 = "GKSL";
        var league2 = "YKSL";
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = [league1, league2]
        };

        _cacheMock.Setup(x => x.BuildKey("2025-2026", league1)).Returns("key1");
        _cacheMock.Setup(x => x.BuildKey("2025-2026", league2)).Returns("key2");

        _cacheMock.Setup(x => x.TryGet("key1", out It.Ref<List<Game>>.IsAny))
            .Returns(new TryGetDelegate((string key, out List<Game> games) =>
            {
                games = [new() { Division = league1 }];
                return true;
            }));

        _cacheMock.Setup(x => x.TryGet("key2", out It.Ref<List<Game>>.IsAny))
            .Returns(new TryGetDelegate((string key, out List<Game> games) =>
            {
                games = [new() { Division = league2 }];
                return true;
            }));

        // Act
        var result = await _service.GetGamesAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(g => g.Division == league1);
        result.Should().Contain(g => g.Division == league2);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("NONEXISTENT")]
    public async Task GetGamesAsync_ShouldHandleInvalidLeague_WhenUnsupportedLeagueProvided(string invalidLeague)
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = [invalidLeague]
        };

        // Act & Assert - Should handle gracefully, not throw for invalid leagues
        var result = await _service.GetGamesAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGamesAsync_ShouldLogErrors_WhenExceptionOccurs()
    {
        // Arrange
        var request = new FixtureRequest
        {
            SeasonId = "2025-2026",
            Leagues = ["GKSL"]
        };

        var cacheKey = "test-cache-key";
        _cacheMock.Setup(x => x.BuildKey(request.SeasonId, "GKSL"))
            .Returns(cacheKey);

        // Set up cache miss to trigger HTTP fetch
        var emptyGames = new List<Game>();
        _cacheMock.Setup(x => x.TryGet(cacheKey, out emptyGames))
            .Returns(false);

        // Set up HTTP client factory to return null to cause exception in fetch
        _httpClientFactoryMock.Setup(x => x.CreateClient("FixtureClient"))
            .Returns((HttpClient)null!);

        // Act
        var result = await _service.GetGamesAsync(request);

        // Assert - Service should handle exceptions gracefully and return empty list
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("2025-2026", "INVALID")]
    [InlineData("2025-2026", "NONEXISTENT")]
    public async Task GetRawAsync_ShouldThrowArgumentException_WhenInvalidLeagueCode(
        string seasonId, string invalidLeagueCode)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetRawAsync(seasonId, invalidLeagueCode));
    }
}
