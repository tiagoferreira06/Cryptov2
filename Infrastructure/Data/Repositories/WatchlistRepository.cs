using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Infrastructure.Data;
using Npgsql;

namespace CryptoPlatform.Infrastructure.Data.Repositories;

public class WatchlistRepository : IWatchlistRepository
{
    private readonly DbConnectionFactory _db;

    public WatchlistRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task AddAsync(Guid userId, string cryptoId)
    {
        const string sql = """
            INSERT INTO watchlists (id, user_id, crypto_id)
            VALUES (@id, @userId, @cryptoId)
            ON CONFLICT (user_id, crypto_id) DO NOTHING;
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("cryptoId", cryptoId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveAsync(Guid userId, string cryptoId)
    {
        const string sql = """
            DELETE FROM watchlists
            WHERE user_id = @userId AND crypto_id = @cryptoId;
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("cryptoId", cryptoId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<string>> GetCryptoIdsAsync(Guid userId)
    {
        const string sql = """
            SELECT crypto_id
            FROM watchlists
            WHERE user_id = @userId;
        """;

        var result = new List<string>();

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }
}
