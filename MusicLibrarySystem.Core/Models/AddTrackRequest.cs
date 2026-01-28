public class AddTrackRequest
{
    public string Title { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int AlbumId { get; set; }
}