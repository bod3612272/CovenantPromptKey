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
            .then((registration) => {
                // Prompt user when an updated SW is ready; reload to pick up new DLL/assets.
                // Use sessionStorage guard to avoid reload loops.
                const reloadKey = 'cpk_sw_reload_pending';

                navigator.serviceWorker.addEventListener('controllerchange', () => {
                    if (sessionStorage.getItem(reloadKey) === '1') {
                        sessionStorage.removeItem(reloadKey);
                        window.location.reload();
                    }
                });

                registration.addEventListener('updatefound', () => {
                    const installingWorker = registration.installing;
                    if (!installingWorker) {
                        return;
                    }

                    installingWorker.addEventListener('statechange', () => {
                        if (installingWorker.state !== 'installed') {
                            return;
                        }

                        // If there's an existing controller, this is an update.
                        if (!navigator.serviceWorker.controller) {
                            return;
                        }

                        // If the new worker is waiting, ask user to reload.
                        const waitingWorker = registration.waiting;
                        if (!waitingWorker) {
                            return;
                        }

                        const shouldReload = window.confirm(
                            'CovenantPromptKey 已有新版可用。要立即重新整理以更新嗎？'
                        );

                        if (!shouldReload) {
                            return;
                        }

                        sessionStorage.setItem(reloadKey, '1');
                        waitingWorker.postMessage({ type: 'SKIP_WAITING' });
                    });
                });

                return registration.update();
            })
            .catch(() => {
                // Intentionally no-op: offline capability is best-effort.
            });
    });
})();
