namespace CryptoPlatform.Domain.Entities;

public class WatchlistItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CryptoId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
