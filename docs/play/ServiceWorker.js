const cacheName = "Couleslaw-Project L-1.0";
const contentToCache = [
    "Build/ae90069a8c44dc7323515f830f8d89e7.loader.js",
    "Build/f20198a4c968120b8efbfdd56f89d11c.framework.js",
    "Build/f0941cf029ef29779241e6ab956898b5.data",
    "Build/1495133f75af79a9beb9f4833e5b2f52.wasm",
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
