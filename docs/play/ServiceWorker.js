const cacheName = "Couleslaw-Project L-1.0";
const contentToCache = [
    "Build/4b3c2150883605d02959d89a37c9abe3.loader.js",
    "Build/f20198a4c968120b8efbfdd56f89d11c.framework.js",
    "Build/cdc9ca5146e72d1429cc2b7e28119c92.data",
    "Build/85f515435e2a8c5e3e0841e757e55ec6.wasm",
    "TemplateData/style.css"

];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
});

self.addEventListener('fetch', function (e) {
    e.respondWith((async function () {
      let response = await caches.match(e.request);
      console.log(`[Service Worker] Fetching resource: ${e.request.url}`);
      if (response) { return response; }

      response = await fetch(e.request);
      const cache = await caches.open(cacheName);
      console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
      cache.put(e.request, response.clone());
      return response;
    })());
});
