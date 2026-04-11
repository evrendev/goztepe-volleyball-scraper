<script setup lang="ts">
import { ref, onMounted } from "vue";
import { fixtureService, standingsService } from "@/services/volleyballService";
import type { CacheStatus } from "@/types";

interface CacheInfo {
  name: string;
  status: CacheStatus | null;
  loading: boolean;
  error: string | null;
}

const caches = ref<CacheInfo[]>([
  { name: "fixtures", status: null, loading: false, error: null },
  { name: "standings", status: null, loading: false, error: null },
]);

const isOpen = ref(false);

async function loadCacheStatus() {
  // Fixture cache
  caches.value[0].loading = true;
  try {
    caches.value[0].status = await fixtureService.getCacheStatus();
    caches.value[0].error = null;
  } catch (e) {
    caches.value[0].error =
      e instanceof Error ? e.message : "Cache durumu alınamadı";
  } finally {
    caches.value[0].loading = false;
  }

  // Standings cache
  caches.value[1].loading = true;
  try {
    caches.value[1].status = await standingsService.getCacheStatus();
    caches.value[1].error = null;
  } catch (e) {
    caches.value[1].error =
      e instanceof Error ? e.message : "Cache durumu alınamadı";
  } finally {
    caches.value[1].loading = false;
  }
}

async function clearCache(type: "fixtures" | "standings", seasonId?: string) {
  const cacheIndex = type === "fixtures" ? 0 : 1;
  const service = type === "fixtures" ? fixtureService : standingsService;

  caches.value[cacheIndex].loading = true;
  try {
    await service.clearCache(seasonId);
    await loadCacheStatus(); // Refresh status
  } catch (e) {
    caches.value[cacheIndex].error =
      e instanceof Error ? e.message : "Cache temizlenemedi";
  } finally {
    caches.value[cacheIndex].loading = false;
  }
}

function getCacheDisplayName(type: string) {
  return type === "fixtures" ? "Fikstür" : "Puan Durumu";
}

onMounted(() => {
  if (isOpen.value) {
    loadCacheStatus();
  }
});

function toggleCache() {
  isOpen.value = !isOpen.value;
  if (isOpen.value && !caches.value.some((c) => c.status)) {
    loadCacheStatus();
  }
}
</script>

<template>
  <!-- Cache Management Toggle Button -->
  <button
    @click="toggleCache"
    class="fixed bottom-4 right-4 bg-gray-600 hover:bg-gray-700 text-white p-3 rounded-full shadow-lg transition-colors z-50"
    title="Cache Yönetimi"
  >
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M4 7v10c0 2.21 1.79 4 4 4h8c2.21 0 4-1.79 4-4V7M4 7c0-2.21 1.79-4 4-4h8c2.21 0 4 1.79 4 4M4 7h16M16 11l-4 4-4-4"
      />
    </svg>
  </button>

  <!-- Cache Management Panel -->
  <div
    v-if="isOpen"
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click.self="isOpen = false"
  >
    <div
      class="bg-white rounded-xl shadow-xl max-w-2xl w-full max-h-96 overflow-y-auto"
    >
      <!-- Header -->
      <div
        class="flex items-center justify-between p-4 border-b border-gray-100"
      >
        <h3 class="text-lg font-semibold text-gray-900">Cache Yönetimi</h3>
        <button
          @click="isOpen = false"
          class="text-gray-400 hover:text-gray-600"
        >
          <svg
            class="w-6 h-6"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
      </div>

      <!-- Cache Lists -->
      <div class="p-4 space-y-6">
        <div
          v-for="(cache, index) in caches"
          :key="cache.name"
          class="space-y-3"
        >
          <div class="flex items-center justify-between">
            <h4 class="font-medium text-gray-900">
              {{ getCacheDisplayName(cache.name) }} Cache
            </h4>
            <div class="flex gap-2">
              <button
                @click="loadCacheStatus"
                :disabled="cache.loading"
                class="text-sm text-goztepe-red hover:text-goztepe-dark disabled:opacity-50"
              >
                Yenile
              </button>
              <button
                @click="clearCache(cache.name as 'fixtures' | 'standings')"
                :disabled="cache.loading"
                class="text-sm text-red-600 hover:text-red-700 disabled:opacity-50"
              >
                Tümünü Temizle
              </button>
            </div>
          </div>

          <!-- Loading -->
          <div
            v-if="cache.loading"
            class="flex items-center gap-2 text-sm text-gray-500"
          >
            <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
              <circle
                class="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                stroke-width="4"
              />
              <path
                class="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8v8H4z"
              />
            </svg>
            Yükleniyor...
          </div>

          <!-- Error -->
          <div
            v-else-if="cache.error"
            class="text-sm text-red-600 bg-red-50 p-2 rounded"
          >
            {{ cache.error }}
          </div>

          <!-- Cache Status -->
          <div v-else-if="cache.status" class="space-y-2">
            <div class="text-sm text-gray-600">
              <span class="font-medium">{{
                cache.status.totalCachedKeys
              }}</span>
              önbellek kaydı
            </div>

            <div v-if="cache.status.keys.length > 0" class="space-y-1">
              <details class="text-sm">
                <summary
                  class="cursor-pointer text-gray-600 hover:text-gray-800"
                >
                  Cache anahtarlarını görüntüle
                </summary>
                <div
                  class="mt-2 space-y-1 bg-gray-50 p-2 rounded text-xs font-mono max-h-32 overflow-y-auto"
                >
                  <div
                    v-for="key in cache.status.keys"
                    :key="key"
                    class="text-gray-700"
                  >
                    {{ key }}
                  </div>
                </div>
              </details>
            </div>

            <div v-else class="text-sm text-gray-500 italic">Cache boş</div>
          </div>

          <!-- Separator -->
          <hr v-if="index < caches.length - 1" class="border-gray-100" />
        </div>
      </div>

      <!-- Footer -->
      <div class="bg-gray-50 p-4 rounded-b-xl">
        <p class="text-xs text-gray-500">
          Cache temizleme işlemi sonrasında veriler yeniden API'den
          çekilecektir.
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
details[open] summary {
  margin-bottom: 0.5rem;
}
</style>
