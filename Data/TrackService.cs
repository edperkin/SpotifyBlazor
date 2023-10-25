using SpotifyAPI.Web;
using TrackRequest = SpotifyBlazor.Models.TrackRequest;

namespace SpotifyBlazor.Data;

public class TrackService
{
    public static List<Track>? MatchingTracks;

    public static async Task GetAudioFeatures(SpotifyClient spotify, SimplePlaylist selectedPlaylist)
    {
        MatchingTracks = null;

        var playlistItems = spotify.Playlists.Get(selectedPlaylist.Id).Result.Tracks?.Items;

        var basicTracks = playlistItems
            .Where(p => p.Track is FullTrack)
            .Select(t => t.Track as FullTrack)
            .ToList();

        var trackIds = basicTracks.Select(t => t.Id).ToList();

        var trackAudioFeaturesList = spotify.Tracks.GetSeveralAudioFeatures(new TracksAudioFeaturesRequest(trackIds)).Result.AudioFeatures;
        
        for (var i = 0; i < trackAudioFeaturesList.Count; i++)
        {
            if (TrackFeaturesInRange(trackAudioFeaturesList[i]))
            {
                Console.WriteLine(basicTracks[i].Name);

                if (MatchingTracks is null)
                {
                    MatchingTracks = new List<Track>
                    {
                        new()
                        {
                            Name = basicTracks[i].Name,
                            Instrumentalness = trackAudioFeaturesList[i].Instrumentalness,
                            Speechiness = trackAudioFeaturesList[i].Speechiness,
                            Valence = trackAudioFeaturesList[i].Valence,
                            Acousticness = trackAudioFeaturesList[i].Acousticness
                        }
                    };
                }
                else
                {
                    MatchingTracks.Add(new Track
                    {
                        Name = basicTracks[i].Name,
                        Instrumentalness = trackAudioFeaturesList[i].Instrumentalness,
                        Speechiness = trackAudioFeaturesList[i].Speechiness,
                        Valence = trackAudioFeaturesList[i].Valence,
                        Acousticness = trackAudioFeaturesList[i].Acousticness
                    });
                }
            }
        }

        Console.WriteLine("Submitted");

        TrackRequest.Instrumentalness = null;
        TrackRequest.Speechiness = null;
        TrackRequest.Valence = null;
        TrackRequest.Acousticness = null;
    }
    
    private static bool CheckFeatures(float? trackRequestFeature, float trackAudioFeature)
    {
        return trackRequestFeature is null || IsFeatureInRange(0.1, trackAudioFeature, trackRequestFeature.Value * 0.01f);
    }

    private static bool IsFeatureInRange(double range, float audioFeature, double request)
    {
        var lowRange = audioFeature - range;
        var highRange = audioFeature + range;
        return request >= lowRange && request <= highRange;
    }

    private static bool TrackFeaturesInRange(TrackAudioFeatures track)
    {
        var isInInstrumentalnessRange = CheckFeatures(TrackRequest.Instrumentalness, track.Instrumentalness);
        var isInSpeechinessRange = CheckFeatures(TrackRequest.Speechiness, track.Speechiness);
        var isInValenceRange = CheckFeatures(TrackRequest.Valence, track.Valence);
        var isInAcousticnessRange = CheckFeatures(TrackRequest.Acousticness, track.Acousticness);

        return isInInstrumentalnessRange & isInSpeechinessRange & isInValenceRange & isInAcousticnessRange;
    }
}