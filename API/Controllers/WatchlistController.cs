using System.Security.Claims;
using CryptoPlatform.Application.DTOs;
using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoPlatform.API.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize] // 👈 Protegido
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistRepository _watchlistRepository;
    private readonly ICoinGeckoService _coinGeckoService;

    public WatchlistController(
        IWatchlistRepository watchlistRepository,
        ICoinGeckoService coinGeckoService)
    {
        _watchlistRepository = watchlistRepository;
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
        await _watchlistRepository.AddAsync(userId, cryptoId);
        return Ok();
    }

    [HttpDelete("{cryptoId}")]
    public async Task<IActionResult> Remove(string cryptoId)
    {
        var userId = GetUserId();
        await _watchlistRepository.RemoveAsync(userId, cryptoId);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        var cryptoIds = await _watchlistRepository.GetCryptoIdsAsync(userId);

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