namespace CryptoPlatform.Application.DTOs;

public class AddTransactionRequest
{
    public string CryptoId { get; set; } = null!;
    public string Type { get; set; } = null!; // "BUY" ou "SELL"
    public decimal Quantity { get; set; }
    public decimal PriceEur { get; set; }
}