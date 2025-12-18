namespace CryptoPlatform.Application.DTOs;

public class WatchlistItemDto
{
    public string CryptoId { get; set; } = null!;
    public decimal CurrentPriceEur { get; set; }
    public decimal Change1h { get; set; }
    public decimal Change24h { get; set; }
}
