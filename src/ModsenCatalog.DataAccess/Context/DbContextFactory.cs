namespace ModsenCatalog.DataAccess.Context;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class DbConnectionFactory
{
    private readonly string connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(connectionString);
    }
}