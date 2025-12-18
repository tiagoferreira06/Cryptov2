using CryptoPlatform.Domain.Enums;

namespace CryptoPlatform.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal PriceEur { get; set; }
    public DateTime CreatedAt { get; set; }
}
