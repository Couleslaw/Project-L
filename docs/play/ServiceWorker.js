const cacheName = "Couleslaw-Project L-1.4.1";
const contentToCache = [
    "Build/fce9658440e7de8c102be61116d96608.loader.js",
    "Build/39884686707199574f32b541e4aa0f9b.framework.js",
    "Build/8b3992fcc332dbd5f3080e5aa5b7174c.data",
    "Build/c41eb51c634488d8cbfda57fb817138f.wasm",
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
