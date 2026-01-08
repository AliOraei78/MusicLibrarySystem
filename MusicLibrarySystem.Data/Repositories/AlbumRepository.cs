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

    // QueryAsync – returns a list (multiple records)
    public async Task<IEnumerable<Album>> GetByArtistAsync(string artist)
    {
        const string sql = @"
        SELECT ""Id"", ""Title"", ""Artist"", ""Year"", ""Rating"" 
        FROM ""Albums"" 
        WHERE ""Artist"" ILIKE @ArtistPattern";

        var parameters = new { ArtistPattern = $"%{artist}%" };  // Contains search (LIKE)

        using var connection = new NpgsqlConnection(_connectionString);
        var albums = await connection.QueryAsync<Album>(sql, parameters);

        return albums;
    }

    // QueryFirstAsync – first record or throws if none exists
    public async Task<Album> GetFirstByYearAsync(int year)
    {
        const string sql = @"
        SELECT ""Id"", ""Title"", ""Artist"", ""Year"", ""Rating"" 
        FROM ""Albums"" 
        WHERE ""Year"" = @Year 
        ORDER BY ""Title"" 
        LIMIT 1";

        using var connection = new NpgsqlConnection(_connectionString);
        var album = await connection.QueryFirstAsync<Album>(sql, new { Year = year });

        return album;
    }

    // QueryFirstOrDefaultAsync – first record or null
    public async Task<Album?> GetFirstOrDefaultByTitleAsync(string title)
    {
        const string sql = @"
        SELECT ""Id"", ""Title"", ""Artist"", ""Year"", ""Rating"" 
        FROM ""Albums"" 
        WHERE ""Title"" ILIKE @Title";

        using var connection = new NpgsqlConnection(_connectionString);
        var album = await connection.QueryFirstOrDefaultAsync<Album>(
            sql, new { Title = $"%{title}%" });

        return album;
    }

    // QuerySingleAsync – exactly one record or throws if zero or more than one exists
    public async Task<Album> GetSingleByIdAsync(int id)
    {
        const string sql = @"
        SELECT ""Id"", ""Title"", ""Artist"", ""Year"", ""Rating"" 
        FROM ""Albums"" 
        WHERE ""Id"" = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var album = await connection.QuerySingleAsync<Album>(sql, new { Id = id });

        return album;
    }

    // QuerySingleOrDefaultAsync – exactly one record or null
    public async Task<Album?> GetSingleOrDefaultByIdAsync(int id)
    {
        const string sql = @"
        SELECT ""Id"", ""Title"", ""Artist"", ""Year"", ""Rating"" 
        FROM ""Albums"" 
        WHERE ""Id"" = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var album = await connection.QuerySingleOrDefaultAsync<Album>(sql, new { Id = id });

        return album;
    }

    public async Task<AlbumWithTracksDto?> GetAlbumWithTracksAsync(int albumId)
    {
        const string sql = @"
        SELECT a.""Id"" AS AlbumId, a.""Title"" AS AlbumTitle, a.""Artist"", a.""Year"", a.""Rating"",
               t.""Id"" AS TrackId, t.""Title"" AS TrackTitle, t.""DurationSeconds""
        FROM ""Albums"" a
        LEFT JOIN ""Tracks"" t ON t.""AlbumId"" = a.""Id""
        WHERE a.""Id"" = @AlbumId";

        using var connection = new NpgsqlConnection(_connectionString);

        var albumDict = new Dictionary<int, AlbumWithTracksDto>();

        var albums = await connection.QueryAsync<AlbumWithTracksDto, Track, AlbumWithTracksDto>(
            sql,
            (album, track) =>
            {
                if (!albumDict.TryGetValue(album.Id, out var albumEntry))
                {
                    albumEntry = album;
                    albumEntry.Tracks = new List<Track>();
                    albumDict.Add(albumEntry.Id, albumEntry);
                }

                if (track != null)
                {
                    albumEntry.Tracks.Add(track);
                }

                return albumEntry;
            },
            new { AlbumId = albumId },
            splitOn: "TrackId"
        );

        return albums.FirstOrDefault();
    }
    public async Task<(IEnumerable<Album> albums, IEnumerable<Track> tracks)> GetAlbumsAndTracksAsync()
    {
        const string sql = @"
        SELECT ""Id"", ""Title"", ""Artist"", ""Year"", ""Rating"" FROM ""Albums"";
        SELECT ""Id"", ""Title"", ""DurationSeconds"", ""AlbumId"" FROM ""Tracks""";

        using var connection = new NpgsqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(sql);

        var albums = await multi.ReadAsync<Album>();
        var tracks = await multi.ReadAsync<Track>();

        return (albums, tracks);
    }

    /// <summary>
    /// QueryBuffered – all results are loaded into memory (Dapper default)
    /// </summary>
    public async Task<IEnumerable<Album>> GetAllBufferedAsync()
    {
        const string sql = @"
        SELECT ""Id"", ""Title"", ""Artist"", ""Year"", ""Rating"" 
        FROM ""Albums"" 
        ORDER BY ""Id""";

        using var connection = new NpgsqlConnection(_connectionString);
        // QueryAsync is buffered by default
        var albums = await connection.QueryAsync<Album>(sql);

        return albums;
    }

    /// <summary>
    /// </summary>
    public async Task<IEnumerable<Album>> GetAllUnbufferedAsync()
    {
        const string sql = @"
        SELECT ""Id"", ""Title"", ""Artist"", ""Year"", ""Rating"" 
        FROM ""Albums"" 
        ORDER BY ""Id""";

        using var connection = new NpgsqlConnection(_connectionString);
        var albumsAsync = connection.QueryUnbufferedAsync<Album>(sql);

        var list = new List<Album>();
        await foreach (var album in albumsAsync)
        {
            list.Add(album);
        }

        return list;
    }
}