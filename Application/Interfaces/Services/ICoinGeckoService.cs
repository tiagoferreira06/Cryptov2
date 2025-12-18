using CryptoPlatform.Application.DTOs;

namespace CryptoPlatform.Application.Interfaces.Services;

public interface ICoinGeckoService
{
    Task<CryptoMarketData> GetMarketDataAsync(string cryptoId);
}
