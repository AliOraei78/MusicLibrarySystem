using Microsoft.AspNetCore.Mvc;
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
}