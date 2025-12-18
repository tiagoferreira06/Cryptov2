namespace CryptoPlatform.Application.Interfaces.Repositories;

public interface IWatchlistRepository
{
    Task AddAsync(Guid userId, string cryptoId);
    Task RemoveAsync(Guid userId, string cryptoId);
    Task<List<string>> GetCryptoIdsAsync(Guid userId);
}
