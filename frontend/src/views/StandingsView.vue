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
            to="/"
            class="px-3 py-2 text-sm font-medium rounded-md text-gray-300 hover:text-white hover:bg-goztepe-red transition-colors"
          >
            Fikstür
          </router-link>
          <router-link
            to="/standings"
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

      <!-- Selected Competition Info -->
      <div
        v-if="store.selectedCompetition"
        class="bg-white rounded-xl shadow-sm border border-gray-100 p-4"
      >
        <div class="flex items-center justify-between">
          <div>
            <h2 class="text-lg font-semibold text-gray-900">
              {{ store.selectedCompetition.displayName }}
            </h2>
            <p class="text-sm text-gray-500 mt-1">
              {{ store.selectedSeason }} • {{ store.selectedCategory }} •
              {{ store.selectedLeagueCode }}
            </p>
          </div>
          <div
            v-if="store.selectedCompetition.hasGoztepe"
            class="flex items-center gap-2 text-goztepe-red"
          >
            <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path
                d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
              />
            </svg>
            <span class="text-sm font-medium">Göztepe Katılıyor</span>
          </div>
        </div>
      </div>

      <!-- Göztepe Quick Stats -->
      <div
        v-if="store.goztepeStandings"
        class="bg-gradient-to-r from-goztepe-red to-goztepe-dark text-white rounded-xl p-6"
      >
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-xl font-bold">Göztepe</h3>
            <p class="text-goztepe-light text-sm">
              {{ store.selectedCompetition?.displayName }}
            </p>
          </div>
          <div class="flex items-center gap-6 text-right">
            <div>
              <div class="text-2xl font-bold">
                {{ store.goztepeStandings.position }}
              </div>
              <div class="text-xs text-goztepe-light">Sıralama</div>
            </div>
            <div>
              <div class="text-2xl font-bold">
                {{ store.goztepeStandings.points }}
              </div>
              <div class="text-xs text-goztepe-light">Puan</div>
            </div>
            <div>
              <div class="text-sm">
                {{ store.goztepeStandings.won }}G -
                {{ store.goztepeStandings.lost }}M
              </div>
              <div class="text-xs text-goztepe-light">
                {{ store.goztepeStandings.played }} Maç
              </div>
            </div>
          </div>
        </div>
      </div>

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

      <!-- Göztepe Games Only (if too many games) -->
      <div
        v-if="
          store.goztepeGames.length > 0 &&
          store.standings?.games &&
          store.goztepeGames.length < store.standings.games.length
        "
        class="space-y-4"
      >
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <h3
            class="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-4"
          >
            Göztepe Maçları ({{ store.goztepeGames.length }})
          </h3>
          <GameResults :games="store.goztepeGames" :loading="false" />
        </div>
      </div>

      <!-- Empty State -->
      <div
        v-if="!store.selectedCompetition && !store.loading"
        class="text-center py-16"
      >
        <svg
          class="mx-auto h-12 w-12 text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="1"
            d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2M12 9l-3 3 3 3m0-6l3 3-3 3"
          />
        </svg>
        <h3 class="mt-4 text-lg font-medium text-gray-900">Yarışma Seçin</h3>
        <p class="mt-2 text-sm text-gray-500">
          Puan durumunu görüntülemek için yukarıdan bir yarışma seçin.
        </p>
      </div>
    </main>

    <!-- Cache Manager -->
    <CacheManager />
  </div>
</template>
