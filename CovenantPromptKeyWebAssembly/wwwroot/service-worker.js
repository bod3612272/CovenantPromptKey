// Development service worker (no offline caching).
// The published variant is emitted as service-worker.published.js.

self.addEventListener('install', (event) => {
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(self.clients.claim());
});
