/* eslint-disable no-restricted-globals */

// Published service worker: offline-first app shell caching.

importScripts('./service-worker-assets.js');

const cachePrefix = 'covenantpromptkey-pwa-cache-';
const cacheName = `${cachePrefix}${self.assetsManifest.version}`;

const precacheUrls = (self.assetsManifest.assets || [])
    .filter((asset) => asset.url)
    .map((asset) => new URL(asset.url, self.location).toString());

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(cacheName).then((cache) => cache.addAll(precacheUrls))
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(
                keys
                    .filter((key) => key.startsWith(cachePrefix) && key !== cacheName)
                    .map((key) => caches.delete(key))
            )
        ).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', (event) => {
    if (event.request.method !== 'GET') {
        return;
    }

    const requestUrl = new URL(event.request.url);
    if (requestUrl.origin !== self.location.origin) {
        return;
    }

    // SPA navigation: serve cached index.html when available.
    if (event.request.mode === 'navigate') {
        event.respondWith(
            caches.match(new URL('index.html', self.location).toString(), { ignoreSearch: true })
                .then((cached) => cached || fetch(event.request))
        );
        return;
    }

    // Cache-first for static assets.
    event.respondWith(
        caches.match(event.request, { ignoreSearch: true })
            .then((cached) => cached || fetch(event.request))
    );
});
