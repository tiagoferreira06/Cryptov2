using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Domain.Entities;
using Npgsql;

namespace CryptoPlatform.Infrastructure.Data.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly DbConnectionFactory _db;

    public PortfolioRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Portfolio?> GetByUserAndCryptoAsync(Guid userId, string cryptoId)
    {
        const string sql = """
            SELECT id, user_id, crypto_id, quantity
            FROM portfolios
            WHERE user_id = @userId AND crypto_id = @cryptoId;
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("cryptoId", cryptoId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Portfolio
        {
            Id = reader.GetGuid(0),
            UserId = reader.GetGuid(1),
            CryptoId = reader.GetString(2),
            Quantity = reader.GetDecimal(3)
        };
    }

    public async Task<List<Portfolio>> GetByUserAsync(Guid userId)
    {
        const string sql = """
            SELECT id, user_id, crypto_id, quantity
            FROM portfolios
            WHERE user_id = @userId AND quantity > 0;
        """;

        var result = new List<Portfolio>();

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new Portfolio
            {
                Id = reader.GetGuid(0),
                UserId = reader.GetGuid(1),
                CryptoId = reader.GetString(2),
                Quantity = reader.GetDecimal(3)
            });
        }

        return result;
    }

    public async Task CreateAsync(Portfolio portfolio)
    {
        const string sql = """
            INSERT INTO portfolios (id, user_id, crypto_id, quantity, created_at)
            VALUES (@id, @userId, @cryptoId, @quantity, @createdAt);
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", portfolio.Id);
        cmd.Parameters.AddWithValue("userId", portfolio.UserId);
        cmd.Parameters.AddWithValue("cryptoId", portfolio.CryptoId);
        cmd.Parameters.AddWithValue("quantity", portfolio.Quantity);
        cmd.Parameters.AddWithValue("createdAt", DateTime.UtcNow);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateQuantityAsync(Guid portfolioId, decimal newQuantity)
    {
        const string sql = """
            UPDATE portfolios
            SET quantity = @quantity
            WHERE id = @id;
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", portfolioId);
        cmd.Parameters.AddWithValue("quantity", newQuantity);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid portfolioId)
    {
        const string sql = """
            DELETE FROM portfolios WHERE id = @id;
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", portfolioId);

        await cmd.ExecuteNonQueryAsync();
    }
}