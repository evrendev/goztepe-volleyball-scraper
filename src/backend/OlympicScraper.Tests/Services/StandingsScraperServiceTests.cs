using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using OlympicScraper.Api.Services;
using OlympicScraper.Api.Models.Standings;

namespace OlympicScraper.Tests.Services;

public class StandingsScraperServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<StandingsScraperService>> _loggerMock;
    private readonly Mock<IStandingsCacheService> _cacheMock;
    private readonly IStandingsScraperService _service;

    public StandingsScraperServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<StandingsScraperService>>();
        _cacheMock = new Mock<IStandingsCacheService>();
        _service = new StandingsScraperService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _cacheMock.Object);
    }

    [Theory]
    [InlineData("2025-2026", "GK", "GKSL")]
    [InlineData("2024-2025", "BB", "BBSL")]
    [InlineData("2023-2024", "YK", "YK1L")]
    public async Task GetCompetitionsAsync_ShouldReturnCachedResults_WhenCacheHit(
        string seasonId, string category, string leagueCode)
    {
        // Arrange
        var request = new CompetitionRequest
        {
            SeasonId = seasonId,
            Category = category,
            LeagueCode = leagueCode
        };

        var expectedCompetitions = new List<Competition>
        {
            new() { Name = "Test Competition", DisplayName = "Test Display Name", LeagueCode = leagueCode }
        };

        var cacheKey = "test-cache-key";
        _cacheMock.Setup(x => x.BuildCompetitionsKey(seasonId, category, leagueCode))
            .Returns(cacheKey);

        _cacheMock.Setup(x => x.TryGetCompetitions(cacheKey, out It.Ref<List<Competition>>.IsAny))
            .Returns((string key, out List<Competition> competitions) =>
            {
                competitions = expectedCompetitions;
                return true;
            });

        // Act
        var result = await _service.GetCompetitionsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCompetitions);
        result.Should().HaveCount(1);
        result.First().LeagueCode.Should().Be(leagueCode);
    }

    [Fact]
    public async Task GetCompetitionsAsync_ShouldReturnEmptyList_WhenHttpFails()
    {
        // Arrange
        var request = new CompetitionRequest
        {
            SeasonId = "2025-2026",
            Category = "GK",
            LeagueCode = "GKSL"
        };

        var cacheKey = "test-cache-key";
        _cacheMock.Setup(x => x.BuildCompetitionsKey(request.SeasonId, request.Category, request.LeagueCode))
            .Returns(cacheKey);

        _cacheMock.Setup(x => x.TryGetCompetitions(cacheKey, out It.Ref<List<Competition>>.IsAny))
            .Returns(false);

        var mockHttpClient = new Mock<HttpClient>();
        _httpClientFactoryMock.Setup(x => x.CreateClient("StandingsClient"))
            .Returns(mockHttpClient.Object);

        // Act
        var result = await _service.GetCompetitionsAsync(request);

        // Assert - Service should return empty list on HTTP failure, not throw
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetCompetitionsAsync_ShouldHandleInvalidInput_WhenSeasonIdInvalid(string invalidSeasonId)
    {
        // Arrange
        var request = new CompetitionRequest
        {
            SeasonId = invalidSeasonId,
            Category = "GK",
            LeagueCode = "GKSL"
        };

        var cacheKey = "test-cache-key";
        _cacheMock.Setup(x => x.BuildCompetitionsKey(request.SeasonId, request.Category, request.LeagueCode))
            .Returns(cacheKey);

        _cacheMock.Setup(x => x.TryGetCompetitions(cacheKey, out It.Ref<List<Competition>>.IsAny))
            .Returns(false);

        // Mock httpClient to prevent real network calls  
        var mockHttpClient = new Mock<HttpClient>();
        _httpClientFactoryMock.Setup(x => x.CreateClient("StandingsClient"))
            .Returns(mockHttpClient.Object);

        // Act & Assert - The method should not throw but might return empty results
        var result = await _service.GetCompetitionsAsync(request);
        result.Should().NotBeNull();
        result.Should().BeOfType<List<Competition>>();
    }
}