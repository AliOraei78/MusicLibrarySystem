using Microsoft.AspNetCore.Mvc;
using MusicLibrarySystem.Core.Models;
using MusicLibrarySystem.Data.Repositories;
using System.Diagnostics;

namespace MusicLibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ControllerBase
{
    private readonly AlbumRepository _albumRepository;
    private readonly ReportRepository _reportRepository;
    private readonly AlbumHybridRepository _albumHybridRepository;

    public AlbumsController(AlbumRepository albumRepository, ReportRepository reportRepository, AlbumHybridRepository albumHybridRepository)
    {
        _albumRepository = albumRepository;
        _reportRepository = reportRepository;
        _albumHybridRepository = albumHybridRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var albums = await _albumRepository.GetAllAsync();
        return Ok(albums);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var album = await _albumRepository.GetByIdAsync(id);
        if (album == null) return NotFound();
        return Ok(album);
    }

    [HttpGet("by-artist/{artist}")]
    public async Task<IActionResult> GetByArtist(string artist)
    {
        var albums = await _albumRepository.GetByArtistAsync(artist);
        return Ok(albums);
    }

    [HttpGet("first-by-year/{year}")]
    public async Task<IActionResult> GetFirstByYear(int year)
    {
        try
        {
            var album = await _albumRepository.GetFirstByYearAsync(year);
            return Ok(album);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"No album was found for the year {year}");
        }
    }

    [HttpGet("single/{id}")]
    public async Task<IActionResult> GetSingle(int id)
    {
        try
        {
            var album = await _albumRepository.GetSingleByIdAsync(id);
            return Ok(album);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Multi-Mapping test: album with its list of tracks
    /// </summary>
    [HttpGet("with-tracks/{albumId}")]
    public async Task<IActionResult> GetAlbumWithTracks(int albumId)
    {
        var album = await _albumRepository.GetAlbumWithTracksAsync(albumId);

        if (album == null)
            return NotFound($"No album found with Id {albumId}");

        return Ok(album);
    }

    /// <summary>
    /// QueryMultiple test: retrieve albums and tracks in a single database call
    /// </summary>
    [HttpGet("albums-and-tracks")]
    public async Task<IActionResult> GetAlbumsAndTracks()
    {
        var (albums, tracks) = await _albumRepository.GetAlbumsAndTracksAsync();

        var result = new
        {
            AlbumsCount = albums.Count(),
            Albums = albums,
            TracksCount = tracks.Count(),
            Tracks = tracks
        };

        return Ok(result);
    }

    /// <summary>
    /// QueryBuffered test – all data is loaded into memory (default behavior)
    /// </summary>
    [HttpGet("buffered")]
    public async Task<IActionResult> GetBuffered()
    {
        var albums = await _albumRepository.GetAllBufferedAsync();

        return Ok(new
        {
            Count = albums.Count(),
            Type = "Buffered (All data loaded in memory)",
            Albums = albums
        });
    }

    /// <summary>
    /// QueryUnbuffered test – data is read in streaming mode
    /// </summary>
    [HttpGet("unbuffered")]
    public async Task<IActionResult> GetUnbuffered()
    {
        var albums = await _albumRepository.GetAllUnbufferedAsync();

        return Ok(new
        {
            Count = albums.Count(),
            Type = "Unbuffered (Streaming - low memory usage)",
            Albums = albums
        });
    }

    /// <summary>
    /// Insert a new album using ExecuteAsync
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAlbum([FromBody] Album album)
    {
        var newId = await _albumRepository.InsertAlbumAsync(album);
        return CreatedAtAction(nameof(GetById), new { id = newId }, album);
    }

    /// <summary>
    /// Update an album using ExecuteAsync
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAlbum(int id, [FromBody] Album album)
    {
        album.Id = id;  // Ensure the correct Id
        var rows = await _albumRepository.UpdateAlbumAsync(album);
        return rows > 0 ? NoContent() : NotFound();
    }

    /// <summary>
    /// Delete an album using ExecuteAsync
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAlbum(int id)
    {
        var rows = await _albumRepository.DeleteAlbumAsync(id);
        return rows > 0 ? NoContent() : NotFound();
    }

    /// <summary>
    /// Add a track via Stored Procedure
    /// </summary>
    [HttpPost("tracks/sp")]
    public async Task<IActionResult> AddTrackViaSP([FromBody] AddTrackRequest request)
    {
        await _albumRepository.AddTrackViaStoredProcedureAsync(
            request.Title,
            request.DurationSeconds,
            request.AlbumId
        );

        return Ok("Track added via Stored Procedure");
    }

    /// <summary>
    /// Add an album and its tracks in a simple transaction (Dapper Transaction)
    /// </summary>
    [HttpPost("transactional")]
    public async Task<IActionResult> AddAlbumTransactional([FromBody] AddAlbumWithTracksRequest request)
    {
        var tracksForRepo = request.Tracks.Select(t => (t.Title, t.Duration)).ToList();

        var albumId = await _albumRepository.AddAlbumWithTracksTransactionalAsync(
            request.AlbumTitle,
            request.Artist,
            request.Year,
            request.Rating,
            tracksForRepo
        );

        return CreatedAtAction(nameof(GetById), new { id = albumId }, request);
    }

    /// <summary>
    /// Add an album and its tracks using TransactionScope
    /// </summary>
    [HttpPost("transaction-scope")]
    public async Task<IActionResult> AddAlbumTransactionScope([FromBody] AddAlbumWithTracksRequest request)
    {
        var tracksForRepo = request.Tracks.Select(t => (t.Title, t.Duration)).ToList();

        var albumId = await _albumRepository.AddAlbumWithTracksTransactionScopeAsync(
            request.AlbumTitle,
            request.Artist,
            request.Year,
            request.Rating,
            tracksForRepo
        );

        return CreatedAtAction(nameof(GetById), new { id = albumId }, request);
    }

    /// <summary>
    /// Batch insert test with 10,000 new tracks (high performance with Dapper)
    /// </summary>
    /// <param name="albumId">The ID of the album to which the tracks will be added</param>
    /// <returns>The number of records inserted and the execution time</returns>
    [HttpPost("batch-insert-test/{albumId}")]
    public async Task<IActionResult> BatchInsertTest(int albumId)
    {
        if (albumId <= 0)
            return BadRequest("AlbumId must be greater than 0");

        var stopwatch = Stopwatch.StartNew();

        var tracks = new List<Track>();
        for (int i = 1; i <= 10000; i++)
        {
            tracks.Add(new Track
            {
                Title = $"Track Batch {i} - {DateTime.UtcNow:HH:mm:ss}",
                DurationSeconds = 180 + (i % 120),
                AlbumId = albumId
            });
        }

        try
        {
            await _albumRepository.InsertBatchTracksAsync(tracks);

            stopwatch.Stop();

            return Ok(new
            {
                Message = "Batch insert successful",
                RecordsInserted = tracks.Count,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                TotalTime = $"{stopwatch.Elapsed.TotalSeconds:F2} seconds"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error during batch insert: {ex.Message}");
        }
    }

    [HttpGet("cached")]
    public async Task<IActionResult> GetCached()
    {
        var albums = await _albumRepository.GetAllCachedAsync();
        return Ok(albums);
    }

    [HttpGet("performance-test")]
    public async Task<IActionResult> PerformanceTest()
    {
        var stopwatch = Stopwatch.StartNew();

        var albums = await _albumRepository.GetAllCachedAsync();

        stopwatch.Stop();

        return Ok(new
        {
            Count = albums.Count(),
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            FromCache = stopwatch.ElapsedMilliseconds < 10
        });
    }

    [HttpGet("auto-all")]
    public async Task<IActionResult> GetAllAuto()
    {
        var albums = await _albumRepository.GetAllAutoAsync();
        return Ok(albums);
    }

    [HttpGet("auto/{id}")]
    public async Task<IActionResult> GetByIdAuto(int id)
    {
        var album = await _albumRepository.GetByIdAutoAsync(id);
        if (album == null) return NotFound();
        return Ok(album);
    }

    [HttpPost("auto")]
    public async Task<IActionResult> CreateAuto([FromBody] Album album)
    {
        var newId = await _albumRepository.InsertAlbumAutoAsync(album);
        return CreatedAtAction(nameof(GetByIdAuto), new { id = newId }, album);
    }

    [HttpPut("auto/{id}")]
    public async Task<IActionResult> UpdateAuto(int id, [FromBody] Album album)
    {
        album.Id = id;
        var success = await _albumRepository.UpdateAlbumAutoAsync(album);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("auto/{id}")]
    public async Task<IActionResult> DeleteAuto(int id)
    {
        var success = await _albumRepository.DeleteAlbumAutoAsync(id);
        return success ? NoContent() : NotFound();
    }

    /// <summary>
    /// Ambient Context test: get the list of albums without manual connection management
    /// Advantages: connection is reused, cleaner code, async/await-safe
    /// </summary>
    [HttpGet("ambient-all")]
    public async Task<IActionResult> GetAllWithAmbient()
    {
        var albums = await _albumRepository.GetAllWithAmbientAsync();

        return Ok(new
        {
            Message = "Ambient Context test succeeded",
            Count = albums.Count(),
            Albums = albums,
            Note = "There is no 'using connection' inside the repository method — the connection was reused from the Ambient Context"
        });
    }

    [HttpGet("multi-db-test")]
    public async Task<IActionResult> MultiDbTest()
    {
        var totalTracks = await _reportRepository.GetTotalTracksCountAsync();
        return Ok(new { TotalTracks = totalTracks });
    }

    /// <summary>
    /// Hybrid: EF Core for loading album + tracks (complex relationships)
    /// </summary>
    [HttpGet("hybrid-ef/{id}")]
    public async Task<IActionResult> GetAlbumHybridEf(int id)
    {
        var album = await _albumHybridRepository.GetAlbumWithTracksEfAsync(id);
        if (album == null) return NotFound();
        return Ok(album);
    }

    /// <summary>
    /// Hybrid: Dapper for fast reporting (Top albums by track count and average duration)
    /// </summary>
    [HttpGet("hybrid-report")]
    public async Task<IActionResult> GetTopAlbumsReport(int topN = 10)
    {
        var report = await _albumHybridRepository.GetTopAlbumsReportDapperAsync(topN);
        return Ok(report);
    }

    [HttpGet("advanced-detail/{albumId}")]
    public async Task<IActionResult> GetAdvancedDetail(int albumId)
    {
        var detail = await _albumRepository.GetAlbumDetailCustomAsync(albumId);
        if (detail == null) return NotFound();
        return Ok(detail);
    }

}