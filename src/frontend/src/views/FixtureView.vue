<script setup lang="ts">
import { onMounted } from "vue";
import { useFixtureStore } from "@/stores/fixture";
import Selector from "@/components/Selector.vue";
import FixtureList from "@/components/FixtureList.vue";
import CacheManager from "@/components/CacheManager.vue";

const store = useFixtureStore();

onMounted(() => {
  store.fetchLeagues();
});
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-goztepe-dark text-white shadow-lg">
      <div
        class="max-w-7xl mx-auto px-4 py-4 flex items-center justify-between"
      >
        <div class="flex items-center gap-3">
          <div
            class="w-10 h-10 bg-goztepe-red rounded-full flex items-center justify-center shrink-0"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-6 w-6"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <circle cx="12" cy="12" r="10" stroke-width="1.5" />
              <path
                stroke-linecap="round"
                stroke-width="1.5"
                d="M12 2C6.5 6 6.5 18 12 22M12 2c5.5 4 5.5 16 0 20M2 12h20"
              />
            </svg>
          </div>
          <div>
            <h1 class="text-lg font-bold leading-tight">Göztepe Voleybol</h1>
            <p class="text-xs text-gray-400">İzmir İl Voleybol Fikstürleri</p>
          </div>
        </div>

        <!-- Navigation Links -->
        <nav class="flex space-x-4">
          <router-link
            to="/"
            class="px-3 py-2 text-sm font-medium rounded-md bg-goztepe-red text-white"
          >
            Fikstür
          </router-link>
          <router-link
            to="/standings"
            class="px-3 py-2 text-sm font-medium rounded-md text-gray-300 hover:text-white hover:bg-goztepe-red transition-colors"
          >
            Puan Durumu
          </router-link>
        </nav>
      </div>
    </header>

    <main class="max-w-7xl mx-auto px-4 py-6 space-y-6">
      <!-- Unified Selector -->
      <Selector mode="fixture" />

      <!-- Fixture Content -->
      <FixtureList />
    </main>

    <!-- Cache Manager -->
    <CacheManager />
  </div>
</template>
