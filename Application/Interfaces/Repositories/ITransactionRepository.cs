using CryptoPlatform.Domain.Entities;

namespace CryptoPlatform.Application.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task CreateAsync(Transaction transaction);
    Task<List<Transaction>> GetByPortfolioAsync(Guid portfolioId);
}