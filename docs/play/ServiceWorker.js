const cacheName = "Couleslaw-Project L-1.2";
const contentToCache = [
    "Build/d3daa95c983dccb9bfebd6cbb1391042.loader.js",
    "Build/f20198a4c968120b8efbfdd56f89d11c.framework.js",
    "Build/843fa9538c8571a781f7b607566a26b8.data",
    "Build/7b88bddbac325b4ba21e62ed13590ac3.wasm",
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
