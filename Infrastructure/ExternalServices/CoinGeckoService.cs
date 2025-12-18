using System.Net.Http.Json;
using CryptoPlatform.Application.DTOs;
using CryptoPlatform.Application.Interfaces.Services;
using CryptoPlatform.Infrastructure.ExternalServices.Models;

namespace CryptoPlatform.Infrastructure.ExternalServices;

public class CoinGeckoService : ICoinGeckoService
{
    private readonly HttpClient _httpClient;

    public CoinGeckoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CryptoMarketData> GetMarketDataAsync(string cryptoId)
    {
        var url =
            $"coins/markets?vs_currency=eur&ids={cryptoId}&price_change_percentage=1h,24h";

        var response = await _httpClient
            .GetFromJsonAsync<List<CoinGeckoMarketResponse>>(url);

        var data = response?.FirstOrDefault();

        if (data == null)
            throw new Exception("Coin not found in CoinGecko");

        return new CryptoMarketData
        {
            Id = data.Id,
            CurrentPriceEur = data.CurrentPrice,
            Change1h = data.Change1h ?? 0,
            Change24h = data.Change24h ?? 0
        };
    }
}
