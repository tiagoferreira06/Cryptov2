namespace CryptoPlatform.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal PriceEur { get; set; }
    public DateTime CreatedAt { get; set; }
}