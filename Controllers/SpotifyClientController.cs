using SpotifyAPI.Web;

namespace SpotifyBlazor.Controllers;

public class SpotifyClientController
{
    public static bool ActiveDevice;
    
    public static bool IsPlaying;
    
    public static async Task ChooseDevice(SpotifyClient spotify, Device device)
    {
        ActiveDevice = await spotify.Player.TransferPlayback(new PlayerTransferPlaybackRequest(new List<string> { device.Id }));
    }

    public static async Task Play(SpotifyClient spotify, SimplePlaylist? selectedPlaylist)
    {
        await spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest
        {
            ContextUri = $"spotify:playlist:{selectedPlaylist.Id}",
            // OffsetParam = _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest()).Result.Item,
            PositionMs = spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest()).Result.ProgressMs
        });
        IsPlaying = true;
    }
    
    public static async Task Pause(SpotifyClient spotify)
    {
        await spotify.Player.PausePlayback();
        IsPlaying = false;
    }

    public static async Task Next(SpotifyClient spotify)
    {
        await spotify.Player.SkipNext();
    }

    public static async Task Previous(SpotifyClient spotify)
    {
        await spotify.Player.SkipPrevious();
    }
}