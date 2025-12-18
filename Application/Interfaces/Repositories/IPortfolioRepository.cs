using CryptoPlatform.Domain.Entities;

namespace CryptoPlatform.Application.Interfaces.Repositories;

public interface IPortfolioRepository
{
    Task<Portfolio?> GetByUserAndCryptoAsync(Guid userId, string cryptoId);
    Task<List<Portfolio>> GetByUserAsync(Guid userId);
    Task CreateAsync(Portfolio portfolio);
    Task UpdateQuantityAsync(Guid portfolioId, decimal newQuantity);
    Task DeleteAsync(Guid portfolioId);
}