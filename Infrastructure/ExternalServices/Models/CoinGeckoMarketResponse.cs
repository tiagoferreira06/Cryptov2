using System.Text.Json.Serialization;

namespace CryptoPlatform.Infrastructure.ExternalServices.Models;

public class CoinGeckoMarketResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("current_price")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("price_change_percentage_1h_in_currency")]
    public decimal? Change1h { get; set; }

    [JsonPropertyName("price_change_percentage_24h_in_currency")]
    public decimal? Change24h { get; set; }
}
