using Dapper;
using Npgsql;
using MusicLibrarySystem.Core.Models;
using Microsoft.Extensions.Configuration;

namespace MusicLibrarySystem.Data.Repositories;

public class AlbumRepository
{
    private readonly string _connectionString;

    public AlbumRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IEnumerable<Album>> GetAllAsync()
    {
        const string sql = "SELECT \"Id\", \"Title\", \"Artist\", \"Year\", \"Rating\" FROM \"Albums\"";

        using var connection = new NpgsqlConnection(_connectionString);
        var albums = await connection.QueryAsync<Album>(sql);

        return albums;
    }

    public async Task<Album?> GetByIdAsync(int id)
    {
        const string sql = "SELECT \"Id\", \"Title\", \"Artist\", \"Year\", \"Rating\" FROM \"Albums\" WHERE \"Id\" = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var album = await connection.QueryFirstOrDefaultAsync<Album>(sql, new { Id = id });

        return album;
    }
}