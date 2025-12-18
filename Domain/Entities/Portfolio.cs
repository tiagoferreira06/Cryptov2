namespace CryptoPlatform.Domain.Entities;

public class Portfolio
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CryptoId { get; set; } = null!;
    public decimal Quantity { get; set; }
}
