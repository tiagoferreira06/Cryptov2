using CryptoPlatform.Application.DTOs;
using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Domain.Entities;
using CryptoPlatform.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CryptoPlatform.DataService.Controllers;

[ApiController]
[Route("data")]
[Produces("application/xml")] // 👈 Retorna XML
public class DataController : ControllerBase
{
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly IWatchlistRepository _watchlistRepo;
    private readonly ITransactionRepository _transactionRepo;

    public DataController(
        IPortfolioRepository portfolioRepo,
        IWatchlistRepository watchlistRepo,
        ITransactionRepository transactionRepo)
    {
        _portfolioRepo = portfolioRepo;
        _watchlistRepo = watchlistRepo;
        _transactionRepo = transactionRepo;
    }

    // ============ PORTFOLIO ============

    [HttpGet("portfolio/{userId}")]
    public async Task<IActionResult> GetPortfolio(Guid userId)
    {
        var portfolios = await _portfolioRepo.GetByUserAsync(userId);
        return Ok(portfolios);
    }

    [HttpGet("portfolio/{userId}/{cryptoId}")]
    public async Task<IActionResult> GetPortfolioItem(Guid userId, string cryptoId)
    {
        var portfolio = await _portfolioRepo.GetByUserAndCryptoAsync(userId, cryptoId);
        if (portfolio == null)
            return NotFound();
        return Ok(portfolio);
    }

    [HttpPost("portfolio")]
    public async Task<IActionResult> CreatePortfolio([FromBody] CreatePortfolioRequest request)
    {
        // Validações
        if (request.UserId == Guid.Empty)
            return BadRequest("UserId inválido");

        if (string.IsNullOrWhiteSpace(request.CryptoId))
            return BadRequest("CryptoId é obrigatório");

        if (request.Quantity < 0)
            return BadRequest("Quantity não pode ser negativa");

        var portfolio = new Portfolio
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CryptoId = request.CryptoId,
            Quantity = request.Quantity
        };

        await _portfolioRepo.CreateAsync(portfolio);
        return Ok(portfolio.Id);
    }

    [HttpPut("portfolio/{portfolioId}")]
    public async Task<IActionResult> UpdatePortfolio(Guid portfolioId, [FromBody] decimal newQuantity)
    {
        // Validações
        if (portfolioId == Guid.Empty)
            return BadRequest("PortfolioId inválido");

        if (newQuantity < 0)
            return BadRequest("Quantity não pode ser negativa");

        await _portfolioRepo.UpdateQuantityAsync(portfolioId, newQuantity);
        return Ok();
    }

    [HttpDelete("portfolio/{portfolioId}")]
    public async Task<IActionResult> DeletePortfolio(Guid portfolioId)
    {
        await _portfolioRepo.DeleteAsync(portfolioId);
        return Ok();
    }

    // ============ WATCHLIST ============

    [HttpGet("watchlist/{userId}")]
    public async Task<IActionResult> GetWatchlist(Guid userId)
    {
        var cryptoIds = await _watchlistRepo.GetCryptoIdsAsync(userId);
        return Ok(cryptoIds);
    }

    [HttpPost("watchlist")]
    public async Task<IActionResult> AddToWatchlist([FromBody] WatchlistRequest request)
    {
        // Validações
        if (request.UserId == Guid.Empty)
            return BadRequest("UserId inválido");

        if (string.IsNullOrWhiteSpace(request.CryptoId))
            return BadRequest("CryptoId é obrigatório");

        await _watchlistRepo.AddAsync(request.UserId, request.CryptoId);
        return Ok();
    }

    [HttpDelete("watchlist")]
    public async Task<IActionResult> RemoveFromWatchlist([FromBody] WatchlistRequest request)
    {
        // Validações
        if (request.UserId == Guid.Empty)
            return BadRequest("UserId inválido");

        if (string.IsNullOrWhiteSpace(request.CryptoId))
            return BadRequest("CryptoId é obrigatório");

        await _watchlistRepo.RemoveAsync(request.UserId, request.CryptoId);
        return Ok();
    }

    // ============ TRANSACTIONS ============

    [HttpPost("transaction")]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        // Validações
        if (request.PortfolioId == Guid.Empty)
            return BadRequest("PortfolioId inválido");

        if (string.IsNullOrWhiteSpace(request.Type))
            return BadRequest("Type é obrigatório");

        if (request.Type != "BUY" && request.Type != "SELL")
            return BadRequest("Type deve ser BUY ou SELL");

        if (request.Quantity <= 0)
            return BadRequest("Quantity deve ser maior que zero");

        if (request.PriceEur <= 0)
            return BadRequest("PriceEur deve ser maior que zero");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PortfolioId = request.PortfolioId,
            Type = Enum.Parse<TransactionType>(request.Type),
            Quantity = request.Quantity,
            PriceEur = request.PriceEur,
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepo.CreateAsync(transaction);
        return Ok(transaction.Id);
    }

    [HttpGet("transaction/{portfolioId}")]
    public async Task<IActionResult> GetTransactions(Guid portfolioId)
    {
        var transactions = await _transactionRepo.GetByPortfolioAsync(portfolioId);
        return Ok(transactions);
    }
}

// Request DTOs
public class CreatePortfolioRequest
{
    public Guid UserId { get; set; }
    public string CryptoId { get; set; } = null!;
    public decimal Quantity { get; set; }
}

public class WatchlistRequest
{
    public Guid UserId { get; set; }
    public string CryptoId { get; set; } = null!;
}

public class CreateTransactionRequest
{
    public Guid PortfolioId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal PriceEur { get; set; }
}