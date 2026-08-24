(function () {
    if (window.__ytmInjected) return;
    window.__ytmInjected = true;

    function getTrackInfo() {
        let title = ''; let artist = ''; let album = ''; let artwork = ''; let isPlaying = false;
        if (navigator.mediaSession && navigator.mediaSession.metadata) {
            title = navigator.mediaSession.metadata.title || '';
            artist = navigator.mediaSession.metadata.artist || '';
            album = navigator.mediaSession.metadata.album || '';
            if (navigator.mediaSession.metadata.artwork && navigator.mediaSession.metadata.artwork.length > 0) {
                artwork = navigator.mediaSession.metadata.artwork[navigator.mediaSession.metadata.artwork.length - 1].src || '';
            }
        }
        if (!title) { const el = document.querySelector('.title.style-scope.ytmusic-player-bar'); title = el ? el.innerText : ''; }
        if (!artist) { const el = document.querySelector('.byline.style-scope.ytmusic-player-bar'); artist = el ? el.innerText : ''; }
        if (!artwork) { const el = document.querySelector('.image.style-scope.ytmusic-player-bar') || document.querySelector('ytmusic-player-bar img'); artwork = el ? el.src : ''; }
        const btn = document.querySelector('#play-pause-button') || document.querySelector('.play-pause-button');
        if (btn) { const l = btn.getAttribute('title') || btn.getAttribute('aria-label') || ''; isPlaying = l.toLowerCase().includes('pause') || l.includes('暫停'); }
        else { const v = document.querySelector('video'); if (v) isPlaying = !v.paused; }

        return { type: 'trackChange', title: title.trim(), artist: artist.trim(), album: album.trim(), artwork: artwork, isPlaying: isPlaying };
    }

    let lastData = '';
    setInterval(function() {
        const info = getTrackInfo();
        const str = JSON.stringify(info);
        if (str !== lastData) {
            lastData = str;
            if (window.chrome && window.chrome.webview) { window.chrome.webview.postMessage(info); }
        }
    }, 500);

    window.__ytmCommand = function (cmd) {
        if (cmd === 'playPause') {
            const btn = document.querySelector('#play-pause-button') || document.querySelector('.play-pause-button');
            if (btn) btn.click();
            else { const v = document.querySelector('video'); if (v) { v.paused ? v.play() : v.pause(); } }
        } else if (cmd === 'next') {
            const btn = document.querySelector('.next-button'); if (btn) btn.click();
        } else if (cmd === 'previous') {
            const btn = document.querySelector('.previous-button'); if (btn) btn.click();
        }
    };
})();
