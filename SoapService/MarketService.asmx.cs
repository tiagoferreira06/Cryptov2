using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Services;

namespace CryptoPlatform.SoapService
{
    [WebService(Namespace = "http://cryptoplatform.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class MarketService : System.Web.Services.WebService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.coingecko.com/api/v3/")
        };

        static MarketService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CryptoPlatform-SOAP-Service");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        [WebMethod(Description = "Obtém resumo de mercado com as top 10 criptomoedas")]
        public MarketSummaryResponse GetMarketSummary()
        {
            try
            {
                var task = Task.Run(async () => await GetMarketDataAsync());
                return task.Result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching market data: {ex.Message}");
            }
        }

        private async Task<MarketSummaryResponse> GetMarketDataAsync()
        {
            var url = "coins/markets?vs_currency=eur&order=market_cap_desc&per_page=10&page=1&sparkline=false&price_change_percentage=24h";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CoinGeckoMarketData>>(json);

            var summary = new MarketSummaryResponse
            {
                GeneratedAt = DateTime.UtcNow,
                TopCryptos = data.Select(c => new CryptoInfo
                {
                    Id = c.Id,
                    Name = c.Name,
                    Symbol = c.Symbol.ToUpper(),
                    PriceEur = c.CurrentPrice,
                    Change24h = c.PriceChangePercentage24h ?? 0,
                    MarketCapEur = c.MarketCap ?? 0
                }).ToList(),
                TotalMarketCapEur = data.Sum(c => c.MarketCap ?? 0)
            };

            return summary;
        }
    }

    public class CoinGeckoMarketData
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("symbol")]
        public string Symbol { get; set; }

        [Newtonsoft.Json.JsonProperty("current_price")]
        public decimal CurrentPrice { get; set; }

        [Newtonsoft.Json.JsonProperty("price_change_percentage_24h")]
        public decimal? PriceChangePercentage24h { get; set; }

        [Newtonsoft.Json.JsonProperty("market_cap")]
        public decimal? MarketCap { get; set; }
    }

    [Serializable]
    public class MarketSummaryResponse
    {
        public DateTime GeneratedAt { get; set; }
        public List<CryptoInfo> TopCryptos { get; set; }
        public decimal TotalMarketCapEur { get; set; }
    }

    [Serializable]
    public class CryptoInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal PriceEur { get; set; }
        public decimal Change24h { get; set; }
        public decimal MarketCapEur { get; set; }
    }
}