using Microsoft.AspNetCore.Mvc;
using MusicLibrarySystem.Core.Models;
using MusicLibrarySystem.Data.Repositories;

namespace MusicLibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ControllerBase
{
    private readonly AlbumRepository _albumRepository;

    public AlbumsController(AlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
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
}