using System.Security.Claims;
using CryptoPlatform.Application.DTOs;
using CryptoPlatform.Application.Interfaces.Services;
using CryptoPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoPlatform.API.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly DataServiceClient _dataService;
    private readonly ICoinGeckoService _coinGeckoService;

    public PortfolioController(
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

    [HttpPost("transaction")]
    public async Task<IActionResult> AddTransaction([FromBody] AddTransactionRequest request)
    {
        var userId = GetUserId();

        if (request.Type != "BUY" && request.Type != "SELL")
            return BadRequest("Type must be BUY or SELL");

        if (request.Quantity <= 0)
            return BadRequest("Quantity must be positive");

        if (request.PriceEur <= 0)
            return BadRequest("Price must be positive");

        // Buscar portfolio via DataService
        var portfolio = await _dataService.GetPortfolioItemAsync(userId, request.CryptoId);

        if (portfolio == null)
        {
            // Criar novo portfolio
            var newPortfolioId = await _dataService.CreatePortfolioAsync(userId, request.CryptoId, 0);
            portfolio = new DataPortfolioItem
            {
                Id = newPortfolioId,
                UserId = userId,
                CryptoId = request.CryptoId,
                Quantity = 0
            };
        }

        // Calcular nova quantidade
        var newQuantity = request.Type == "BUY"
            ? portfolio.Quantity + request.Quantity
            : portfolio.Quantity - request.Quantity;

        if (newQuantity < 0)
            return BadRequest("Insufficient quantity to sell");

        // Atualizar quantidade
        if (newQuantity == 0)
        {
            await _dataService.DeletePortfolioAsync(portfolio.Id);
        }
        else
        {
            await _dataService.UpdatePortfolioQuantityAsync(portfolio.Id, newQuantity);
        }

        // Registar transação
        await _dataService.CreateTransactionAsync(
            portfolio.Id,
            request.Type,
            request.Quantity,
            request.PriceEur
        );

        return Ok(new { message = "Transaction added successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetPortfolio()
    {
        var userId = GetUserId();
        var portfolios = await _dataService.GetPortfolioAsync(userId);

        var result = new List<PortfolioItemDto>();

        foreach (var portfolio in portfolios)
        {
            var marketData = await _coinGeckoService.GetMarketDataAsync(portfolio.CryptoId);

            result.Add(new PortfolioItemDto
            {
                CryptoId = portfolio.CryptoId,
                Quantity = portfolio.Quantity,
                CurrentPriceEur = marketData.CurrentPriceEur,
                CurrentValueEur = portfolio.Quantity * marketData.CurrentPriceEur,
                Change24h = marketData.Change24h
            });
        }

        return Ok(result);
    }

    [HttpGet("{cryptoId}/transactions")]
    public async Task<IActionResult> GetTransactions(string cryptoId)
    {
        var userId = GetUserId();
        var portfolio = await _dataService.GetPortfolioItemAsync(userId, cryptoId);

        if (portfolio == null)
            return NotFound("Portfolio not found for this crypto");

        var transactions = await _dataService.GetTransactionsAsync(portfolio.Id);

        var result = transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            Type = t.Type,
            Quantity = t.Quantity,
            PriceEur = t.PriceEur,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Ok(result);
    }
}