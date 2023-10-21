using SpotifyAPI.Web;
using SpotifyBlazor.Models;

namespace SpotifyBlazor.Data;

public class TrackService
{
    public static List<Track>? MatchingTracks;

    public static void GetAudioFeatures(SpotifyClient spotify, SimplePlaylist selectedPlaylist)
    {
        MatchingTracks = null;

        foreach (var item in spotify.Playlists.Get(selectedPlaylist.Id).Result.Tracks?.Items)
        {
            if (item.Track is not FullTrack track) continue;

            var trackAudioFeatures = spotify.Tracks.GetAudioFeatures(track.Id).Result;

            var instrumentalnessRange = GetRange(0.1, trackAudioFeatures.Instrumentalness, TrackRequirements.Instrumentalness);
            var speechinessRange = GetRange(0.1, trackAudioFeatures.Speechiness, TrackRequirements.Speechiness);
            var valenceRange = GetRange(0.1, trackAudioFeatures.Valence, TrackRequirements.Valence);

            if (instrumentalnessRange && speechinessRange && valenceRange)
            {
                Console.WriteLine(track.Name + " Instrumentalness:" + trackAudioFeatures.Instrumentalness +
                                  " Speechiness:" + trackAudioFeatures.Speechiness + " Valence:" +
                                  trackAudioFeatures.Valence);

                if (MatchingTracks is null)
                {
                    MatchingTracks = new List<Track>
                    {
                        new()
                        {
                            Name = track.Name,
                            Instrumentalness = trackAudioFeatures.Instrumentalness,
                            Speechiness = trackAudioFeatures.Speechiness,
                            Valence = trackAudioFeatures.Valence
                        }
                    };
                }
                else
                {
                    MatchingTracks.Add(new Track
                    {
                        Name = track.Name,
                        Instrumentalness = trackAudioFeatures.Instrumentalness,
                        Speechiness = trackAudioFeatures.Speechiness,
                        Valence = trackAudioFeatures.Valence
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