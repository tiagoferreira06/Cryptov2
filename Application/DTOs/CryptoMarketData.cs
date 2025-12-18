namespace CryptoPlatform.Application.DTOs;

public class CryptoMarketData
{
    public string Id { get; set; } = null!;
    public decimal CurrentPriceEur { get; set; }
    public decimal Change1h { get; set; }
    public decimal Change24h { get; set; }
}
