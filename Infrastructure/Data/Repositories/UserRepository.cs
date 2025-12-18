using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Domain.Entities;
using Npgsql;

namespace CryptoPlatform.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _db;

    public UserRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = """
            SELECT id, email, password_hash, created_at
            FROM users
            WHERE email = @email;
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("email", email);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            Id = reader.GetGuid(0),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            CreatedAt = reader.GetDateTime(3)
        };
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT id, email, password_hash, created_at
            FROM users
            WHERE id = @id;
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            Id = reader.GetGuid(0),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            CreatedAt = reader.GetDateTime(3)
        };
    }

    public async Task CreateAsync(User user)
    {
        const string sql = """
            INSERT INTO users (id, email, password_hash, created_at)
            VALUES (@id, @email, @passwordHash, @createdAt);
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", user.Id);
        cmd.Parameters.AddWithValue("email", user.Email);
        cmd.Parameters.AddWithValue("passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("createdAt", user.CreatedAt);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string sql = """
            SELECT COUNT(*) FROM users WHERE email = @email;
        """;

        await using var conn = _db.Create();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("email", email);

        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }
}