<script setup lang="ts">
import { onMounted } from "vue";
import { useStandingsStore } from "@/stores/standings";
import CompetitionSelector from "@/components/CompetitionSelector.vue";
import StandingsTable from "@/components/StandingsTable.vue";
import GameResults from "@/components/GameResults.vue";
import CacheManager from "@/components/CacheManager.vue";

const store = useStandingsStore();

onMounted(() => {
  // İlk yüklemede yarışmaları getir
  store.fetchCompetitions();
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
              <path
                stroke-linecap="round"
                stroke-width="1.5"
                d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z"
              />
            </svg>
          </div>
          <div>
            <h1 class="text-lg font-bold leading-tight">Göztepe Voleybol</h1>
            <p class="text-xs text-gray-400">Puan Durumu & Maç Sonuçları</p>
          </div>
        </div>

        <!-- Navigation Links -->
        <nav class="flex space-x-4">
          <router-link
            :to="{ name: 'fixture' }"
            class="px-3 py-2 text-sm font-medium rounded-md text-gray-300 hover:text-white hover:bg-goztepe-red transition-colors"
          >
            Fikstür
          </router-link>
          <router-link
            :to="{ name: 'standings' }"
            class="px-3 py-2 text-sm font-medium rounded-md bg-goztepe-red text-white"
          >
            Puan Durumu
          </router-link>
        </nav>
      </div>
    </header>

    <main class="max-w-7xl mx-auto px-4 py-6 space-y-6">
      <!-- Competition Selector -->
      <CompetitionSelector />

      <!-- Standings and Results -->
      <div v-if="store.standings" class="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <!-- Standings Table -->
        <div>
          <StandingsTable
            :standings="store.standings.standings"
            :loading="store.loading"
          />
        </div>

        <!-- Game Results -->
        <div>
          <GameResults
            :games="store.standings.games"
            :loading="store.loading"
          />
        </div>
      </div>
    </main>

    <!-- Cache Manager -->
    <CacheManager />
  </div>
</template>
