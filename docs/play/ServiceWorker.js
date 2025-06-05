const cacheName = "Couleslaw-Project L-1.3";
const contentToCache = [
    "Build/a6294eedeea04312524f70af646d9cb2.loader.js",
    "Build/f20198a4c968120b8efbfdd56f89d11c.framework.js",
    "Build/7919e58afe04fdcf148881020bdcf625.data",
    "Build/3d2da692d87974918c4058699b0878ac.wasm",
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
