using System.Security.Claims;
using CryptoPlatform.Application.DTOs;
using CryptoPlatform.Application.Interfaces.Services;
using CryptoPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoPlatform.API.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize]
public class WatchlistController : ControllerBase
{
    private readonly DataServiceClient _dataService;
    private readonly ICoinGeckoService _coinGeckoService;

    public WatchlistController(
        DataServiceClient dataService,
        ICoinGeckoService coinGeckoService)
    {
        _dataService = dataService;
        _coinGeckoService = coinGeckoService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost("{cryptoId}")]
    public async Task<IActionResult> Add(string cryptoId)
    {
        var userId = GetUserId();
        await _dataService.AddToWatchlistAsync(userId, cryptoId);
        return Ok();
    }

    [HttpDelete("{cryptoId}")]
    public async Task<IActionResult> Remove(string cryptoId)
    {
        var userId = GetUserId();
        await _dataService.RemoveFromWatchlistAsync(userId, cryptoId);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        var cryptoIds = await _dataService.GetWatchlistAsync(userId);

        var result = new List<WatchlistItemDto>();

        foreach (var cryptoId in cryptoIds)
        {
            var marketData = await _coinGeckoService.GetMarketDataAsync(cryptoId);

            result.Add(new WatchlistItemDto
            {
                CryptoId = cryptoId,
                CurrentPriceEur = marketData.CurrentPriceEur,
                Change1h = marketData.Change1h,
                Change24h = marketData.Change24h
            });
        }

        return Ok(result);
    }
}