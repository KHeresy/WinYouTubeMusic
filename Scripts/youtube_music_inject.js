(function () {
    if (window.__ytmInjected) return;
    window.__ytmInjected = true;

    function getAlbumBounds() {
        const selectors = [
            'ytmusic-player-bar img.image',
            'ytmusic-player-bar .image',
            '#song-image img',
            '.thumbnail-image-wrapper img',
            '#player-bar-background'
        ];

        for (const sel of selectors) {
            const el = document.querySelector(sel);
            if (el) {
                const rect = el.getBoundingClientRect();
                if (rect.width > 20 && rect.height > 20 && rect.top >= 0 && rect.left >= 0) {
                    return {
                        left: Math.round(rect.left),
                        top: Math.round(rect.top),
                        width: Math.round(rect.width),
                        height: Math.round(rect.height)
                    };
                }
            }
        }
        return null;
    }

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
        if (!artwork) { const el = document.querySelector('.image.style-scope.ytmusic-player-bar'); artwork = el ? el.src : ''; }
        const btn = document.querySelector('#play-pause-button') || document.querySelector('.play-pause-button');
        if (btn) { const l = btn.getAttribute('title') || btn.getAttribute('aria-label') || ''; isPlaying = l.toLowerCase().includes('pause') || l.includes('暫停'); }
        else { const v = document.querySelector('video'); if (v) isPlaying = !v.paused; }

        const bounds = getAlbumBounds();

        return { type: 'trackChange', title: title.trim(), artist: artist.trim(), album: album.trim(), artwork: artwork, isPlaying: isPlaying, bounds: bounds };
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
