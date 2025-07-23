const cacheName = "Couleslaw-Project L-1.3.1";
const contentToCache = [
    "Build/4539dde7d79582d6e7b96faf7c1f0e75.loader.js",
    "Build/39884686707199574f32b541e4aa0f9b.framework.js",
    "Build/cdd00d073e3fe7589596c76510ef9eef.data",
    "Build/201a5252ee840946374d1a98a0a0a0cd.wasm",
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
