(function () {
    if (window.__ytmInjected) return;
    window.__ytmInjected = true;

    function getTrackInfo() {
        let title = "";
        let artist = "";
        let album = "";
        let artwork = "";
        let isPlaying = false;

        if (navigator.mediaSession && navigator.mediaSession.metadata) {
            title = navigator.mediaSession.metadata.title || "";
            artist = navigator.mediaSession.metadata.artist || "";
            album = navigator.mediaSession.metadata.album || "";
            if (navigator.mediaSession.metadata.artwork && navigator.mediaSession.metadata.artwork.length > 0) {
                artwork = navigator.mediaSession.metadata.artwork[navigator.mediaSession.metadata.artwork.length - 1].src || "";
            }
        }

        if (!title) {
            const titleEl = document.querySelector('.title.style-scope.ytmusic-player-bar');
            title = titleEl ? titleEl.innerText : "";
        }
        if (!artist) {
            const bylineEl = document.querySelector('.byline.style-scope.ytmusic-player-bar');
            artist = bylineEl ? bylineEl.innerText : "";
        }
        if (!artwork) {
            const imgEl = document.querySelector('.image.style-scope.ytmusic-player-bar');
            artwork = imgEl ? imgEl.src : "";
        }

        const playPauseBtn = document.querySelector('#play-pause-button') || document.querySelector('.play-pause-button');
        if (playPauseBtn) {
            const label = playPauseBtn.getAttribute('title') || playPauseBtn.getAttribute('aria-label') || "";
            isPlaying = label.toLowerCase().includes('pause') || label.includes('暫停');
        } else {
            const video = document.querySelector('video');
            if (video) isPlaying = !video.paused;
        }

        return {
            type: 'trackChange',
            title: title.trim(),
            artist: artist.trim(),
            album: album.trim(),
            artwork: artwork,
            isPlaying: isPlaying
        };
    }

    let lastData = "";
    function sendUpdate() {
        const info = getTrackInfo();
        const str = JSON.stringify(info);
        if (str !== lastData) {
            lastData = str;
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(info);
            }
        }
    }

    setInterval(sendUpdate, 500);

    window.__ytmCommand = function (cmd) {
        if (cmd === 'playPause') {
            const btn = document.querySelector('#play-pause-button') || document.querySelector('.play-pause-button');
            if (btn) btn.click();
            else {
                const video = document.querySelector('video');
                if (video) { video.paused ? video.play() : video.pause(); }
            }
        } else if (cmd === 'next') {
            const btn = document.querySelector('.next-button');
            if (btn) btn.click();
        } else if (cmd === 'previous') {
            const btn = document.querySelector('.previous-button');
            if (btn) btn.click();
        }
    };
})();
