function ClickPlay() {
    const spotifyEmbedWindow = document.querySelector('iframe[src*="spotify.com/embed"]').contentWindow;
    spotifyEmbedWindow.postMessage({command: 'toggle'}, '*');

    const playPause = document.getElementById("play-pause");
    if (playPause.textContent === "Play") {
        playPause.textContent = "Pause";
    } else {
        playPause.textContent = "Play";
    }
}