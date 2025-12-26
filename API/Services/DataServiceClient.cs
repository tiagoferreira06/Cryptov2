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
        try
        {
            var response = await _httpClient.GetAsync($"{_dataServiceUrl}/data/portfolio/{userId}");
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();
            return ParsePortfolioXml(xml);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao buscar portfolio: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro inesperado ao buscar portfolio: {ex.Message}", ex);
        }
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
        try
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

            if (doc.Root?.Value == null)
                throw new Exception("Resposta XML inválida do DataService");

            return Guid.Parse(doc.Root.Value);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao criar portfolio: {ex.Message}", ex);
        }
        catch (FormatException ex)
        {
            throw new Exception($"Erro ao processar resposta do DataService: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro inesperado ao criar portfolio: {ex.Message}", ex);
        }
    }

    public async Task UpdatePortfolioQuantityAsync(Guid portfolioId, decimal newQuantity)
    {
        try
        {
            var content = new StringContent(
                newQuantity.ToString(),
                System.Text.Encoding.UTF8,
                "application/xml"
            );

            var response = await _httpClient.PutAsync($"{_dataServiceUrl}/data/portfolio/{portfolioId}", content);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao atualizar portfolio: {ex.Message}", ex);
        }
    }

    public async Task DeletePortfolioAsync(Guid portfolioId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_dataServiceUrl}/data/portfolio/{portfolioId}");
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao deletar portfolio: {ex.Message}", ex);
        }
    }

    // ============ WATCHLIST ============

    public async Task<List<string>> GetWatchlistAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_dataServiceUrl}/data/watchlist/{userId}");
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();
            return ParseWatchlistXml(xml);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao buscar watchlist: {ex.Message}", ex);
        }
    }

    public async Task AddToWatchlistAsync(Guid userId, string cryptoId)
    {
        try
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
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao adicionar à watchlist: {ex.Message}", ex);
        }
    }

    public async Task RemoveFromWatchlistAsync(Guid userId, string cryptoId)
    {
        try
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
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao remover da watchlist: {ex.Message}", ex);
        }
    }

    // ============ TRANSACTIONS ============

    public async Task<Guid> CreateTransactionAsync(Guid portfolioId, string type, decimal quantity, decimal priceEur)
    {
        try
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

            if (doc.Root?.Value == null)
                throw new Exception("Resposta XML inválida do DataService");

            return Guid.Parse(doc.Root.Value);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao criar transação: {ex.Message}", ex);
        }
        catch (FormatException ex)
        {
            throw new Exception($"Erro ao processar resposta do DataService: {ex.Message}", ex);
        }
    }

    public async Task<List<DataTransactionItem>> GetTransactionsAsync(Guid portfolioId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_dataServiceUrl}/data/transaction/{portfolioId}");
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();
            return ParseTransactionsXml(xml);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Erro ao buscar transações: {ex.Message}", ex);
        }
    }

    // ============ PARSERS ============

    private List<DataPortfolioItem> ParsePortfolioXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.Descendants("Portfolio")
                .Select(p => new DataPortfolioItem
                {
                    Id = Guid.Parse(p.Element("Id")?.Value ?? throw new Exception("Id não encontrado no XML")),
                    UserId = Guid.Parse(p.Element("UserId")?.Value ?? throw new Exception("UserId não encontrado no XML")),
                    CryptoId = p.Element("CryptoId")?.Value ?? throw new Exception("CryptoId não encontrado no XML"),
                    Quantity = decimal.Parse(p.Element("Quantity")?.Value ?? throw new Exception("Quantity não encontrado no XML"))
                }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao fazer parse do XML de portfolio: {ex.Message}", ex);
        }
    }

    private DataPortfolioItem ParseSinglePortfolioXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var p = doc.Root ?? throw new Exception("XML root não encontrado");
            return new DataPortfolioItem
            {
                Id = Guid.Parse(p.Element("Id")?.Value ?? throw new Exception("Id não encontrado no XML")),
                UserId = Guid.Parse(p.Element("UserId")?.Value ?? throw new Exception("UserId não encontrado no XML")),
                CryptoId = p.Element("CryptoId")?.Value ?? throw new Exception("CryptoId não encontrado no XML"),
                Quantity = decimal.Parse(p.Element("Quantity")?.Value ?? throw new Exception("Quantity não encontrado no XML"))
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao fazer parse do XML de portfolio item: {ex.Message}", ex);
        }
    }

    private List<string> ParseWatchlistXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.Descendants("string")
                .Select(s => s.Value)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao fazer parse do XML de watchlist: {ex.Message}", ex);
        }
    }

    private List<DataTransactionItem> ParseTransactionsXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.Descendants("Transaction")
                .Select(t => new DataTransactionItem
                {
                    Id = Guid.Parse(t.Element("Id")?.Value ?? throw new Exception("Id não encontrado no XML")),
                    PortfolioId = Guid.Parse(t.Element("PortfolioId")?.Value ?? throw new Exception("PortfolioId não encontrado no XML")),
                    Type = t.Element("Type")?.Value ?? throw new Exception("Type não encontrado no XML"),
                    Quantity = decimal.Parse(t.Element("Quantity")?.Value ?? throw new Exception("Quantity não encontrado no XML")),
                    PriceEur = decimal.Parse(t.Element("PriceEur")?.Value ?? throw new Exception("PriceEur não encontrado no XML")),
                    CreatedAt = DateTime.Parse(t.Element("CreatedAt")?.Value ?? throw new Exception("CreatedAt não encontrado no XML"))
                }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao fazer parse do XML de transações: {ex.Message}", ex);
        }
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