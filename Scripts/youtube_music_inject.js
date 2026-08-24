(function () {
    if (window.__ytmInjected) return;
    window.__ytmInjected = true;

    function getTrackAndPlaylistInfo() {
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

        let videoId = '';
        let playlistId = '';
        let playlistTitle = '';

        try {
            const player = document.getElementById('movie_player') || (document.querySelector('ytmusic-player') && document.querySelector('ytmusic-player').player_);
            if (player) {
                if (typeof player.getVideoData === 'function') {
                    const data = player.getVideoData();
                    if (data) {
                        videoId = data.video_id || '';
                        if (data.list) playlistId = data.list;
                    }
                }
                if (!playlistId && typeof player.getPlaylistId === 'function') {
                    playlistId = player.getPlaylistId() || '';
                }
            }
        } catch (e) {}

        // Fallback for playlistId from URL
        if (!playlistId && window.location.search) {
            const match = window.location.search.match(/[?&]list=([^&]+)/);
            if (match) playlistId = match[1];
        }

        // Try to get Playlist or Album title from Queue or Player bar
        try {
            const queueHeader = document.querySelector('ytmusic-player-page #header .title') ||
                                document.querySelector('ytmusic-queue-header-renderer .title') ||
                                document.querySelector('.queue-title');
            if (queueHeader && queueHeader.innerText) {
                playlistTitle = queueHeader.innerText.trim();
            }
        } catch (e) {}

        if (!playlistTitle && album) {
            playlistTitle = album.trim();
        }

        if (!playlistTitle) {
            try {
                const albumLink = document.querySelector('ytmusic-player-bar .subtitle a[href*="list="]') ||
                                  document.querySelector('ytmusic-player-bar .subtitle a[href*="browse/"]') ||
                                  document.querySelector('ytmusic-player-bar .byline a[href*="browse/"]');
                if (albumLink && albumLink.innerText) {
                    playlistTitle = albumLink.innerText.trim();
                }
            } catch (e) {}
        }

        // Construct playable track URL (preserving playlist queue if present)
        let trackUrl = '';
        if (videoId) {
            trackUrl = 'https://music.youtube.com/watch?v=' + videoId + (playlistId ? '&list=' + playlistId : '');
        } else if (window.location.href && window.location.href.includes('watch?v=')) {
            trackUrl = window.location.href;
        }

        let playlistUrl = '';
        if (playlistId) {
            playlistUrl = 'https://music.youtube.com/watch?list=' + playlistId;
        }

        return {
            type: 'trackChange',
            title: title.trim(),
            artist: artist.trim(),
            album: album.trim(),
            artwork: artwork,
            isPlaying: isPlaying,
            url: trackUrl,
            playlistId: playlistId,
            playlistTitle: playlistTitle,
            playlistUrl: playlistUrl
        };
    }

    function checkPlaylistPage() {
        try {
            if (window.location.pathname.includes('/playlist') && window.location.search.includes('list=')) {
                const listMatch = window.location.search.match(/[?&]list=([^&]+)/);
                const pId = listMatch ? listMatch[1] : '';
                const titleEl = document.querySelector('ytmusic-responsive-header-renderer .title') ||
                                document.querySelector('ytmusic-header-renderer .title') ||
                                document.querySelector('h1.title') ||
                                document.querySelector('.title.ytmusic-detail-header-renderer');
                const pTitle = titleEl ? titleEl.innerText.trim() : '';
                if (pId && pTitle && window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        type: 'playlistDiscovered',
                        playlistId: pId,
                        playlistTitle: pTitle,
                        playlistUrl: 'https://music.youtube.com/watch?list=' + pId
                    });
                }
            }
        } catch (e) {}
    }

    let lastData = '';
    setInterval(function() {
        const info = getTrackAndPlaylistInfo();
        const str = JSON.stringify(info);
        if (str !== lastData) {
            lastData = str;
            if (window.chrome && window.chrome.webview) { window.chrome.webview.postMessage(info); }
        }
        checkPlaylistPage();
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
