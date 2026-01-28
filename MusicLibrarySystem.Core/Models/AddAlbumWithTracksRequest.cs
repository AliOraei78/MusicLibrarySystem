public class AddAlbumWithTracksRequest
{
    public string AlbumTitle { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Rating { get; set; }

    public List<TrackItem> Tracks { get; set; } = new();

    public class TrackItem
    {
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; } 
    }
}