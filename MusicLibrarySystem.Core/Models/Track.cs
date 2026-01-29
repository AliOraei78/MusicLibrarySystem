using Dapper.Contrib.Extensions;

namespace MusicLibrarySystem.Core.Models;

[Table("\"Tracks\"")]
public class Track
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int DurationSeconds { get; set; }

    public int AlbumId { get; set; }

    public Album? Album { get; set; }
}