using CryptoPlatform.Domain.Entities;

namespace CryptoPlatform.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task CreateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
}