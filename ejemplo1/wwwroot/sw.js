// Cambiamos a v2 para forzar la actualización de la caché
const CACHE_NAME = 'losandes-cache-v2';

const urlsToCache = [
    '/',
    // Archivos locales
    '/css/site.css',
    '/js/site.js',
    '/manifest.json',
    '/icons/icon-192x192.png',
    '/icons/icon-512x512.png',
    // Archivos externos (Bootstrap y Fuentes)
    'https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css',
    'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css',
    'https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap'
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('Archivos y Bootstrap cacheados correctamente en v2');
                return cache.addAll(urlsToCache);
            })
    );
    self.skipWaiting(); // Obliga a que la nueva versión se active rápido
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => {
                return response || fetch(event.request);
            })
    );
});