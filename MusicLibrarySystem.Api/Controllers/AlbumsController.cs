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
}