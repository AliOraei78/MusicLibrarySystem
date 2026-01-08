namespace MusicLibrarySystem.Core.Models;

public class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int AlbumId { get; set; }
}