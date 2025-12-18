namespace CryptoPlatform.Application.DTOs;

public class PortfolioItemDto
{
    public string CryptoId { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal CurrentPriceEur { get; set; }
    public decimal CurrentValueEur { get; set; }
    public decimal Change24h { get; set; }
}