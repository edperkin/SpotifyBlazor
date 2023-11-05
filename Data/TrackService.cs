using SpotifyAPI.Web;
using TrackRequest = SpotifyBlazor.Models.TrackRequest;

namespace SpotifyBlazor.Data;

public static class TrackService
{
    public static List<Track>? MatchingTracks;

    public static async Task GetAudioFeatures(SpotifyClient spotify, SimplePlaylist selectedPlaylist)
    {
        MatchingTracks = null;

        const int limit = 100; 
        const int offset = 0; 
        var basicTracks = new List<FullTrack>();
        var request = new PlaylistGetItemsRequest { Limit = limit, Offset = offset };

        while (true)
        {
            var response = await spotify.Playlists.GetItems(selectedPlaylist.Id, request);
            var items = response.Items.Where(p => p.Track is FullTrack)
                .Select(t => t.Track as FullTrack)
                .ToList();

            basicTracks.AddRange(items);

            if (response.Next != null)
            {
                request.Offset += limit;
            }
            else
            {
                break;
            }
        }

        var trackIdsChunks = basicTracks.Select(t => t.Id).ToList().ChunkBy(100);
        var trackAudioFeaturesList = new List<TrackAudioFeatures>();

        foreach (var trackIds in trackIdsChunks)
        {
            var features =
                await spotify.Tracks.GetSeveralAudioFeatures(new TracksAudioFeaturesRequest(trackIds as IList<string>));
            trackAudioFeaturesList.AddRange(features.AudioFeatures);
        }

        AddMatchingTracksToTrackList(trackAudioFeaturesList, basicTracks);

        Console.WriteLine("Submitted");

        TrackRequest.Instrumentalness = null;
        TrackRequest.Speechiness = null;
        TrackRequest.Valence = null;
        TrackRequest.Acousticness = null;
    }

    private static void AddMatchingTracksToTrackList(IReadOnlyList<TrackAudioFeatures>? trackAudioFeaturesList,  IReadOnlyList<FullTrack>? basicTracks)
    {
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
                            Uri = basicTracks[i].Uri,
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
                        Uri = basicTracks[i].Uri,
                        Instrumentalness = trackAudioFeaturesList[i].Instrumentalness,
                        Speechiness = trackAudioFeaturesList[i].Speechiness,
                        Valence = trackAudioFeaturesList[i].Valence,
                        Acousticness = trackAudioFeaturesList[i].Acousticness
                    });
                }
            }
        }
    }

    private static bool CheckFeatures(float? trackRequestFeature, float trackAudioFeature)
    {
        return trackRequestFeature is null ||
               IsFeatureInRange(0.1, trackAudioFeature, trackRequestFeature.Value * 0.01f);
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

    private static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList());
    }
}