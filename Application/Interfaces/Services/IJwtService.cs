namespace CryptoPlatform.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email);
}