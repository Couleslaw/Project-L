const cacheName = "Couleslaw-Project L-1.4";
const contentToCache = [
    "Build/1ec60878c9790c741b1a26b7098f659d.loader.js",
    "Build/39884686707199574f32b541e4aa0f9b.framework.js",
    "Build/1dce7f4635df2387a15dd855bcd02ea1.data",
    "Build/6f39e565fddcee0fd3bf859a8a012816.wasm",
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
