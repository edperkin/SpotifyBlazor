using SpotifyAPI.Web;
using SpotifyBlazor.Models;

namespace SpotifyBlazor.Data;

public class TrackService
{
    public static List<Track>? MatchingTracks;
    
    public static void GetFeatures(SpotifyClient spotify, SimplePlaylist selectedPlaylist)
    {
        MatchingTracks = null;

        foreach (var item in spotify.Playlists.Get(selectedPlaylist.Id).Result.Tracks?.Items)
        {
            if (item.Track is not FullTrack track) continue;

            var trackFeatures = spotify.Tracks.GetAudioFeatures(track.Id).Result;

            if (trackFeatures.Instrumentalness > TrackRequirements.Instrumentalness && trackFeatures.Speechiness < TrackRequirements.Speechiness)
            {
                Console.WriteLine(track.Name + " Instrumentalness:" + trackFeatures.Instrumentalness + " Speechiness:" + trackFeatures.Speechiness);

                if (MatchingTracks is null)
                {
                    MatchingTracks = new List<Track>
                    {
                        new()
                        {
                            Name = track.Name,
                            Instrumentalness = trackFeatures.Instrumentalness,
                            Speechiness = trackFeatures.Speechiness
                        }
                    };
                }
                else
                {
                    MatchingTracks.Add(new Track
                    {
                        Name = track.Name,
                        Instrumentalness = trackFeatures.Instrumentalness,
                        Speechiness = trackFeatures.Speechiness
                    });
                }
            }
        }
        
        Console.WriteLine("Submitted");
    }
}