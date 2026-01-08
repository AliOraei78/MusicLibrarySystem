namespace MusicLibrarySystem.Core.Models;

public class AlbumWithTracksDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Rating { get; set; }

    public List<Track> Tracks { get; set; } = new();
}