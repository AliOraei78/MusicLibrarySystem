using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MusicLibrarySystem.Data.Ambient;

public class DapperAmbientContext : IDisposable
{
    private readonly IDbConnection _connection;
    private bool _disposed;

    public DapperAmbientContext(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        _connection.Open();
    }

    public IDbConnection Connection => _connection;

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
    }
}