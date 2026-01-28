using Dapper;
using Microsoft.Extensions.Configuration;
using MusicLibrarySystem.Core.Models;
using Npgsql;
using System.Transactions;

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

    /// <summary>
    /// Insert a new album using ExecuteAsync
    /// </summary>
    public async Task<int> InsertAlbumAsync(Album album)
    {
        const string sql = @"
        INSERT INTO ""Albums"" (""Title"", ""Artist"", ""Year"", ""Rating"")
        VALUES (@Title, @Artist, @Year, @Rating)
        RETURNING ""Id""";

        using var connection = new NpgsqlConnection(_connectionString);
        var newId = await connection.ExecuteScalarAsync<int>(sql, album);

        return newId;
    }

    /// <summary>
    /// Update an album using ExecuteAsync
    /// </summary>
    public async Task<int> UpdateAlbumAsync(Album album)
    {
        const string sql = @"
        UPDATE ""Albums""
        SET ""Title"" = @Title,
            ""Artist"" = @Artist,
            ""Year"" = @Year,
            ""Rating"" = @Rating
        WHERE ""Id"" = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, album);

        return rowsAffected;
    }

    /// <summary>
    /// Delete an album using ExecuteAsync
    /// </summary>
    public async Task<int> DeleteAlbumAsync(int id)
    {
        const string sql = @"DELETE FROM ""Albums"" WHERE ""Id"" = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected;
    }

    /// <summary>
    /// Call a stored procedure using Execute
    /// </summary>
    public async Task AddTrackViaStoredProcedureAsync(string title, int durationSeconds, int albumId)
    {
        const string sql = "CALL add_track_and_update_album(@Title, @DurationSeconds, @AlbumId)";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            Title = title,
            DurationSeconds = durationSeconds,
            AlbumId = albumId
        });
    }

    /// <summary>
    /// Add an album and its tracks in a single simple transaction using Dapper
    /// </summary>
    public async Task<int> AddAlbumWithTracksTransactionalAsync(
        string albumTitle,
        string artist,
        int year,
        decimal rating,
        List<(string title, int duration)> tracks)
    {
        const string insertAlbumSql = @"
        INSERT INTO ""Albums"" (""Title"", ""Artist"", ""Year"", ""Rating"")
        VALUES (@Title, @Artist, @Year, @Rating)
        RETURNING ""Id""";

        const string insertTrackSql = @"
        INSERT INTO ""Tracks"" (""Title"", ""DurationSeconds"", ""AlbumId"")
        VALUES (@Title, @DurationSeconds, @AlbumId)";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            // Step 1: Insert the album
            var albumId = await connection.ExecuteScalarAsync<int>(
                insertAlbumSql,
                new
                {
                    Title = albumTitle,
                    Artist = artist,
                    Year = year,
                    Rating = rating
                },
                transaction);

            if (tracks == null || !tracks.Any())
                throw new ArgumentException("At least one track must be added.");

            // Step 2: Insert the tracks
            foreach (var track in tracks)
            {
                await connection.ExecuteAsync(
                    insertTrackSql,
                    new
                    {
                        Title = track.title,
                        DurationSeconds = track.duration,
                        AlbumId = albumId
                    },
                    transaction);
            }

            // Commit
            await transaction.CommitAsync();

            return albumId;
        }
        catch
        {
            // Rollback on error
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Add an album and its tracks using TransactionScope (distributed transaction)
    /// </summary>
    public async Task<int> AddAlbumWithTracksTransactionScopeAsync(
        string albumTitle,
        string artist,
        int year,
        decimal rating,
        List<(string title, int duration)> tracks)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            const string insertAlbumSql = @"
            INSERT INTO ""Albums"" (""Title"", ""Artist"", ""Year"", ""Rating"")
            VALUES (@Title, @Artist, @Year, @Rating)
            RETURNING ""Id""";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var albumId = await connection.ExecuteScalarAsync<int>(insertAlbumSql, new
            {
                Title = albumTitle,
                Artist = artist,
                Year = year,
                Rating = rating
            });

            const string insertTrackSql = @"
            INSERT INTO ""Tracks"" (""Title"", ""DurationSeconds"", ""AlbumId"")
            VALUES (@Title, @DurationSeconds, @AlbumId)";

            foreach (var track in tracks)
            {
                await connection.ExecuteAsync(insertTrackSql, new
                {
                    Title = track.title,
                    DurationSeconds = track.duration,
                    AlbumId = albumId
                });
            }

            scope.Complete(); // Commit
            return albumId;
        }
        catch
        {
            scope.Dispose(); // Rollback
            throw;
        }
    }
}