using System.Xml.Linq;

namespace CryptoPlatform.API.Services;

public class DataServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _dataServiceUrl;

    public DataServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _dataServiceUrl = configuration["DataService:Url"] ?? "https://localhost:5001"; // Ajusta a porta
    }

    // ============ PORTFOLIO ============

    public async Task<List<DataPortfolioItem>> GetPortfolioAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"{_dataServiceUrl}/data/portfolio/{userId}");
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync();
        return ParsePortfolioXml(xml);
    }

    public async Task<DataPortfolioItem?> GetPortfolioItemAsync(Guid userId, string cryptoId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_dataServiceUrl}/data/portfolio/{userId}/{cryptoId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync();
            return ParseSinglePortfolioXml(xml);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Guid> CreatePortfolioAsync(Guid userId, string cryptoId, decimal quantity)
    {
        var content = new StringContent(
            $@"<CreatePortfolioRequest>
                <UserId>{userId}</UserId>
                <CryptoId>{cryptoId}</CryptoId>
                <Quantity>{quantity}</Quantity>
              </CreatePortfolioRequest>",
            System.Text.Encoding.UTF8,
            "application/xml"
        );

        var response = await _httpClient.PostAsync($"{_dataServiceUrl}/data/portfolio", content);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xml);
        return Guid.Parse(doc.Root!.Value);
    }

    public async Task UpdatePortfolioQuantityAsync(Guid portfolioId, decimal newQuantity)
    {
        var content = new StringContent(
            newQuantity.ToString(),
            System.Text.Encoding.UTF8,
            "application/xml"
        );

        var response = await _httpClient.PutAsync($"{_dataServiceUrl}/data/portfolio/{portfolioId}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePortfolioAsync(Guid portfolioId)
    {
        var response = await _httpClient.DeleteAsync($"{_dataServiceUrl}/data/portfolio/{portfolioId}");
        response.EnsureSuccessStatusCode();
    }

    // ============ WATCHLIST ============

    public async Task<List<string>> GetWatchlistAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"{_dataServiceUrl}/data/watchlist/{userId}");
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync();
        return ParseWatchlistXml(xml);
    }

    public async Task AddToWatchlistAsync(Guid userId, string cryptoId)
    {
        var content = new StringContent(
            $@"<WatchlistRequest>
                <UserId>{userId}</UserId>
                <CryptoId>{cryptoId}</CryptoId>
              </WatchlistRequest>",
            System.Text.Encoding.UTF8,
            "application/xml"
        );

        var response = await _httpClient.PostAsync($"{_dataServiceUrl}/data/watchlist", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveFromWatchlistAsync(Guid userId, string cryptoId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_dataServiceUrl}/data/watchlist")
        {
            Content = new StringContent(
                $@"<WatchlistRequest>
                    <UserId>{userId}</UserId>
                    <CryptoId>{cryptoId}</CryptoId>
                  </WatchlistRequest>",
                System.Text.Encoding.UTF8,
                "application/xml"
            )
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    // ============ TRANSACTIONS ============

    public async Task<Guid> CreateTransactionAsync(Guid portfolioId, string type, decimal quantity, decimal priceEur)
    {
        var content = new StringContent(
            $@"<CreateTransactionRequest>
                <PortfolioId>{portfolioId}</PortfolioId>
                <Type>{type}</Type>
                <Quantity>{quantity}</Quantity>
                <PriceEur>{priceEur}</PriceEur>
              </CreateTransactionRequest>",
            System.Text.Encoding.UTF8,
            "application/xml"
        );

        var response = await _httpClient.PostAsync($"{_dataServiceUrl}/data/transaction", content);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xml);
        return Guid.Parse(doc.Root!.Value);
    }

    public async Task<List<DataTransactionItem>> GetTransactionsAsync(Guid portfolioId)
    {
        var response = await _httpClient.GetAsync($"{_dataServiceUrl}/data/transaction/{portfolioId}");
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync();
        return ParseTransactionsXml(xml);
    }

    // ============ PARSERS ============

    private List<DataPortfolioItem> ParsePortfolioXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        return doc.Descendants("Portfolio")
            .Select(p => new DataPortfolioItem
            {
                Id = Guid.Parse(p.Element("Id")!.Value),
                UserId = Guid.Parse(p.Element("UserId")!.Value),
                CryptoId = p.Element("CryptoId")!.Value,
                Quantity = decimal.Parse(p.Element("Quantity")!.Value)
            }).ToList();
    }

    private DataPortfolioItem ParseSinglePortfolioXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        var p = doc.Root!;
        return new DataPortfolioItem
        {
            Id = Guid.Parse(p.Element("Id")!.Value),
            UserId = Guid.Parse(p.Element("UserId")!.Value),
            CryptoId = p.Element("CryptoId")!.Value,
            Quantity = decimal.Parse(p.Element("Quantity")!.Value)
        };
    }

    private List<string> ParseWatchlistXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        return doc.Descendants("string")
            .Select(s => s.Value)
            .ToList();
    }

    private List<DataTransactionItem> ParseTransactionsXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        return doc.Descendants("Transaction")
            .Select(t => new DataTransactionItem
            {
                Id = Guid.Parse(t.Element("Id")!.Value),
                PortfolioId = Guid.Parse(t.Element("PortfolioId")!.Value),
                Type = t.Element("Type")!.Value,
                Quantity = decimal.Parse(t.Element("Quantity")!.Value),
                PriceEur = decimal.Parse(t.Element("PriceEur")!.Value),
                CreatedAt = DateTime.Parse(t.Element("CreatedAt")!.Value)
            }).ToList();
    }
}

// DTOs internos
// DTOs internos (renomeados para evitar conflitos)
public class DataPortfolioItem  // ← ERA PortfolioItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CryptoId { get; set; } = null!;
    public decimal Quantity { get; set; }
}

public class DataTransactionItem  // ← ERA TransactionItemDto
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal PriceEur { get; set; }
    public DateTime CreatedAt { get; set; }
}