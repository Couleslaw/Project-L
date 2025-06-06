const cacheName = "Couleslaw-Project L-1.3";
const contentToCache = [
    "Build/a5d7cfd037d05f8dca37e9e60944b4b7.loader.js",
    "Build/f20198a4c968120b8efbfdd56f89d11c.framework.js",
    "Build/50979faea9956853bfeeeb5c1ef6b6e2.data",
    "Build/60b31a590c8daf13c2cd6b2bb3740fa1.wasm",
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
