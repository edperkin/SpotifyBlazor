using SpotifyAPI.Web;

namespace SpotifyBlazor.Data;

public class SpotifyService
{
    private static IConfiguration _secrets;

    public SpotifyService()
    {
        _secrets = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("secrets.json")
            .Build();
    }
    
    public async Task<Uri> GetUser()
    {
        var loginRequest = new LoginRequest(
            new Uri("http://localhost:5543"),
            _secrets["spotify_client_id"],
            LoginRequest.ResponseType.Token
        )
        {
            Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative }
        };
        return loginRequest.ToUri();
    }
    
    public async Task GetUserPlaylists(SpotifyClient spotify)
    {
        var me = await spotify.UserProfile.Current();
        Console.WriteLine($"Hello there {me.DisplayName}");

        await foreach(var playlist in spotify.Paginate(await spotify.Playlists.CurrentUsers()))
        {
            Console.WriteLine(playlist.Name);
        }
    }

    public async Task GetSong(SpotifyClient spotify)
    {
        //https://open.spotify.com/track/4H0KLsPpr5atperrHGfz3x?si=d8dffcf6e6244739

        var track = await spotify.Tracks.Get("0s8wR0kbIbyJcSNcqR08zD");
        Console.WriteLine(track.Name);
    }
    
    public async Task<List<Spotify>> GetPlaylist()
    {
        var config = SpotifyClientConfig.CreateDefault();

        var request =
            new ClientCredentialsRequest(_secrets["spotify_client_id"], _secrets["spotify_client_secret"]);
        var response = await new OAuthClient(config).RequestToken(request);

        var spotify = new SpotifyClient(config.WithToken(response.AccessToken));
        
        // https://open.spotify.com/playlist/1Lwg5nyCap7Sj10Q81O9nK?si=dae1c0ababc74920
        var playlist = await spotify.Playlists.Get("1Lwg5nyCap7Sj10Q81O9nK");
        Console.WriteLine(playlist.Name);

        var trackList = new List<Spotify>();
        
        foreach (var item in playlist.Tracks?.Items)
        {
            if (item.Track is FullTrack track)
            {
                var t = TimeSpan.FromMilliseconds(track.DurationMs);
                
                var trackDetails = new Spotify
                {
                    Song = track.Name,
                    Album = track.Album.Name,
                    TrackLength = Math.Round(t.TotalMinutes, 2),
                    Popularity = track.Popularity,
                };
                
                trackList.Add(trackDetails);
            }
        }

        return trackList;
    }
}