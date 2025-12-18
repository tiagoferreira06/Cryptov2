using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CryptoPlatform.Infrastructure.Data;

public class DbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public NpgsqlConnection Create()
    {
        return new NpgsqlConnection(
            _configuration.GetConnectionString("Postgres")
        );
    }
}
