using Dapper.Contrib.Extensions;

namespace MusicLibrarySystem.Core.Models;

[Table("\"Albums\"")]
public class Album
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public int Year { get; set; }

    public decimal Rating { get; set; }

    public List<Track> Tracks { get; set; } = new();

    // If you want a computed property (read-only, calculated by the database)
    [Computed]
    public int TrackCount { get; set; }
}
