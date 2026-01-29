using Dapper;
using MusicLibrarySystem.Core.Models;
using MusicLibrarySystem.Data.Context;
using System;
using Microsoft.EntityFrameworkCore;

public class AlbumHybridRepository
{
    private readonly AppDbContext _efContext;
    private readonly IDapperContext _dapperContext;

    public AlbumHybridRepository(AppDbContext efContext, IDapperContext dapperContext)
    {
        _efContext = efContext;
        _dapperContext = dapperContext;
    }

    // Use EF Core for complex operations (relationships, tracking, LINQ)
    public async Task<Album?> GetAlbumWithTracksEfAsync(int id)
    {
        return await _efContext.Albums
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    // Use Dapper for heavy / fast reporting queries
    public async Task<IEnumerable<AlbumReportDto>> GetTopAlbumsReportDapperAsync(int topN = 10)
    {
        const string sql = @"
            SELECT a.""Id"", a.""Title"", a.""Artist"",
                   COUNT(t.""Id"") AS TrackCount,
                   AVG(t.""DurationSeconds"") AS AvgDuration
            FROM ""Albums"" a
            LEFT JOIN ""Tracks"" t ON t.""AlbumId"" = a.""Id""
            GROUP BY a.""Id"", a.""Title"", a.""Artist""
            ORDER BY TrackCount DESC
            LIMIT @TopN";

        using var conn = _dapperContext.CreateConnection();
        return await conn.QueryAsync<AlbumReportDto>(sql, new { TopN = topN });
    }

    // DTO for reporting (simple and lightweight)
    public class AlbumReportDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int TrackCount { get; set; }
        public double AvgDuration { get; set; }
    }
}
