using Microsoft.AspNetCore.Mvc;
using VoleybolScraper.Api.Models;
using VoleybolScraper.Api.Services;

namespace VoleybolScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoleybolController : ControllerBase
{
    private readonly VoleybolScraperService _scraper;

    public VoleybolController(VoleybolScraperService scraper)
        => _scraper = scraper;

    // Desteklenen kümeleri listele
    [HttpGet("leagues")]
    public IActionResult GetLeagues() =>
        Ok(SupportedLeagues.All.Select(l => new
        {
            l.Code,
            l.DisplayName,
            l.Category
        }));

    // Maçları getir
    [HttpPost("matches")]
    public async Task<IActionResult> GetMatches([FromBody] FiksturRequest request)
    {
        // KurumId ve Gender sabit — dışarıdan override edilemez
        request.KurumId = "662";
        request.Gender = "B";

        var matches = await _scraper.GetMaclarAsync(request);

        return Ok(new
        {
            total = matches.Count,
            season = request.SezonId,
            leagues = matches.GroupBy(m => m.League)
                             .Select(g => new
                             {
                                 league = g.Key,
                                 count = g.Count(),
                                 matches = g.ToList()
                             })
        });
    }
}