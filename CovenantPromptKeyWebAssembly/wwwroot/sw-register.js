(() => {
    if (!('serviceWorker' in navigator)) {
        return;
    }

    if (location.hostname === 'localhost' || location.hostname === '127.0.0.1') {
        // Debug ergonomics: avoid stale SW/caches serving old index/assets.
        try {
            navigator.serviceWorker.getRegistrations().then((registrations) => {
                for (const registration of registrations) {
                    registration.unregister();
                }
            });

            if ('caches' in window) {
                caches.keys().then((keys) => {
                    for (const key of keys) {
                        caches.delete(key);
                    }
                });
            }
        } catch {
            // Intentionally no-op
        }

        return;
    }

    window.addEventListener('load', () => {
        navigator.serviceWorker
            .register('service-worker.js', { updateViaCache: 'none' })
            .then((registration) => registration.update())
            .catch(() => {
                // Intentionally no-op: offline capability is best-effort.
            });
    });
})();
