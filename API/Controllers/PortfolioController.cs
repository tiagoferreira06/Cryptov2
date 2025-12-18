using System.Security.Claims;
using CryptoPlatform.Application.DTOs;
using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Application.Interfaces.Services;
using CryptoPlatform.Domain.Entities;
using CryptoPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoPlatform.API.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize] // 👈 Protegido
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICoinGeckoService _coinGeckoService;

    public PortfolioController(
        IPortfolioRepository portfolioRepository,
        ITransactionRepository transactionRepository,
        ICoinGeckoService coinGeckoService)
    {
        _portfolioRepository = portfolioRepository;
        _transactionRepository = transactionRepository;
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

        var portfolio = await _portfolioRepository.GetByUserAndCryptoAsync(userId, request.CryptoId);

        if (portfolio == null)
        {
            portfolio = new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CryptoId = request.CryptoId,
                Quantity = 0
            };
            await _portfolioRepository.CreateAsync(portfolio);
        }

        var newQuantity = request.Type == "BUY"
            ? portfolio.Quantity + request.Quantity
            : portfolio.Quantity - request.Quantity;

        if (newQuantity < 0)
            return BadRequest("Insufficient quantity to sell");

        if (newQuantity == 0)
        {
            await _portfolioRepository.DeleteAsync(portfolio.Id);
        }
        else
        {
            await _portfolioRepository.UpdateQuantityAsync(portfolio.Id, newQuantity);
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            Type = Enum.Parse<TransactionType>(request.Type),
            Quantity = request.Quantity,
            PriceEur = request.PriceEur,
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepository.CreateAsync(transaction);

        return Ok(new { message = "Transaction added successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetPortfolio()
    {
        var userId = GetUserId();
        var portfolios = await _portfolioRepository.GetByUserAsync(userId);

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
        var portfolio = await _portfolioRepository.GetByUserAndCryptoAsync(userId, cryptoId);

        if (portfolio == null)
            return NotFound("Portfolio not found for this crypto");

        var transactions = await _transactionRepository.GetByPortfolioAsync(portfolio.Id);

        var result = transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Quantity = t.Quantity,
            PriceEur = t.PriceEur,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Ok(result);
    }
}