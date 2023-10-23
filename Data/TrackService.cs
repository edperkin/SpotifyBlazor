using SpotifyAPI.Web;
using SpotifyBlazor.Models;
using TrackRequest = SpotifyBlazor.Models.TrackRequest;

namespace SpotifyBlazor.Data;

public class TrackService
{
    public static List<Track>? MatchingTracks;

    public static async Task GetAudioFeatures(SpotifyClient spotify, SimplePlaylist selectedPlaylist)
    {
        MatchingTracks = null;

        var playlistItems =  spotify.Playlists.Get(selectedPlaylist.Id).Result.Tracks?.Items;
        
        var basicTracks = playlistItems
            .Where(p => p.Track is FullTrack)
            .Select(t => t.Track as FullTrack)
            .ToList();
        
        var trackIds = basicTracks.Select(t => t.Id).ToList();
        
        var audioFeatures = await spotify.Tracks.GetSeveralAudioFeatures(new TracksAudioFeaturesRequest(trackIds));
        
        Console.WriteLine(audioFeatures);

        for (int i = 0; i < audioFeatures.AudioFeatures.Count; i++)
        {
            var track = audioFeatures.AudioFeatures[i];
            
            var instrumentalnessRange = GetRange(0.1, track.Instrumentalness, (float)(TrackRequest.Instrumentalness * 0.01));
            var speechinessRange = GetRange(0.1, track.Speechiness, (float)(TrackRequest.Speechiness * 0.01));
            var valenceRange = GetRange(0.1, track.Valence, (float)(TrackRequest.Valence * 0.01));

            if (instrumentalnessRange && speechinessRange && valenceRange)
            {
                Console.WriteLine(track + " Instrumentalness:" + track.Instrumentalness +
                                  " Speechiness:" + track.Speechiness + " Valence:" +
                                  track.Valence);
                
                if (MatchingTracks is null)
                {
                    MatchingTracks = new List<Track>
                    {
                        new()
                        {
                            Name = basicTracks[i].Name,
                            Instrumentalness = track.Instrumentalness,
                            Speechiness = track.Speechiness,
                            Valence = track.Valence
                        }
                    };
                }
                else
                {
                    MatchingTracks.Add(new Track
                    {
                        Name = basicTracks[i].Name,
                        Instrumentalness = track.Instrumentalness,
                        Speechiness = track.Speechiness,
                        Valence = track.Valence
                    });
                }
            }
        }

        Console.WriteLine("Submitted");
    }

    private static bool GetRange(double range, float audioFeature, float requirement)
    {
        return requirement >= audioFeature - range && requirement <= audioFeature + range;
    }
}