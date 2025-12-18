using CryptoPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoPlatform.API.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController : ControllerBase
{
    private readonly ICoinGeckoService _coinGeckoService;

    public MarketController(ICoinGeckoService coinGeckoService)
    {
        _coinGeckoService = coinGeckoService;
    }

    [HttpGet("{cryptoId}")]
    public async Task<IActionResult> GetMarketData(string cryptoId)
    {
        var data = await _coinGeckoService.GetMarketDataAsync(cryptoId);
        return Ok(data);
    }
}
