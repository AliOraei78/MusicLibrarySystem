public class AlbumDetailDto
{
    public int AlbumId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Rating { get; set; }
    public int TrackCount { get; set; }
    public double AverageDuration { get; set; }
    public List<TrackDto> Tracks { get; set; } = new();
}