namespace SpotifyBlazor.Data;

public class Track
{
    public string Name { get; set; }
    public string Uri { get; set; }
    public float Instrumentalness { get; set; }
    public float Speechiness { get; set; }
    public float Valence { get; set; }
    public float Acousticness { get; set; }
}