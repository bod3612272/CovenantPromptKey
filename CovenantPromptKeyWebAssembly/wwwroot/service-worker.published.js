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

    // Activate updated worker ASAP so new assets are picked up without requiring users
    // to close all tabs.
    self.skipWaiting();
});

self.addEventListener('message', (event) => {
    if (event?.data?.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
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

    // SPA navigation: network-first for index.html to avoid serving stale app shells.
    if (event.request.mode === 'navigate') {
        const indexUrl = new URL('index.html', self.location).toString();
        event.respondWith(
            (async () => {
                try {
                    // Always attempt to revalidate index.html.
                    const response = await fetch(indexUrl, { cache: 'no-store' });

                    const cache = await caches.open(cacheName);
                    await cache.put(indexUrl, response.clone());

                    return response;
                } catch {
                    const cached = await caches.match(indexUrl, { ignoreSearch: true });
                    if (cached) {
                        return cached;
                    }

                    return fetch(event.request);
                }
            })()
        );
        return;
    }

    // Blazor boot manifest is not fingerprinted; cache-first can pin clients to an
    // old manifest that references removed hashed assets after a deployment.
    if (requestUrl.pathname.endsWith('/_framework/blazor.boot.json')) {
        event.respondWith(
            (async () => {
                try {
                    const response = await fetch(event.request, { cache: 'no-store' });
                    const cache = await caches.open(cacheName);
                    await cache.put(event.request, response.clone());
                    return response;
                } catch {
                    const cached = await caches.match(event.request, { ignoreSearch: true });
                    if (cached) {
                        return cached;
                    }

                    return fetch(event.request);
                }
            })()
        );
        return;
    }

    // Cache-first for static assets.
    event.respondWith(
        caches.match(event.request, { ignoreSearch: true })
            .then((cached) => cached || fetch(event.request))
    );
});
