using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Domain.Entities;
using CryptoPlatform.Domain.Enums;
using Npgsql;

namespace CryptoPlatform.Infrastructure.Data.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly DbConnectionFactory _db;

    public TransactionRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAsync(Transaction transaction)
    {
        const string sql = """
            INSERT INTO transactions (id, portfolio_id, type, quantity, price_eur, created_at)
            VALUES (@id, @portfolioId, @type, @quantity, @priceEur, @createdAt);
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", transaction.Id);
        cmd.Parameters.AddWithValue("portfolioId", transaction.PortfolioId);
        cmd.Parameters.AddWithValue("type", transaction.Type.ToString());
        cmd.Parameters.AddWithValue("quantity", transaction.Quantity);
        cmd.Parameters.AddWithValue("priceEur", transaction.PriceEur);
        cmd.Parameters.AddWithValue("createdAt", transaction.CreatedAt);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<Transaction>> GetByPortfolioAsync(Guid portfolioId)
    {
        const string sql = """
            SELECT id, portfolio_id, type, quantity, price_eur, created_at
            FROM transactions
            WHERE portfolio_id = @portfolioId
            ORDER BY created_at DESC;
        """;

        var result = new List<Transaction>();

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("portfolioId", portfolioId);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new Transaction
            {
                Id = reader.GetGuid(0),
                PortfolioId = reader.GetGuid(1),
                Type = Enum.Parse<TransactionType>(reader.GetString(2)),
                Quantity = reader.GetDecimal(3),
                PriceEur = reader.GetDecimal(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }

        return result;
    }
}