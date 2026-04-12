<script setup lang="ts">
import { useFixtureStore } from "@/stores/fixture";
import GameCard from "./GameCard.vue";

const store = useFixtureStore();
</script>

<template>
  <!-- Loading -->
  <div
    v-if="store.loading"
    class="flex flex-col items-center justify-center py-20 text-gray-400"
  >
    <svg
      class="animate-spin h-8 w-8 mb-3 text-goztepe-red"
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle
        class="opacity-25"
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        stroke-width="4"
      />
      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
    </svg>
    <p class="text-sm">Fikstür yükleniyor...</p>
  </div>

  <!-- Error -->
  <div
    v-else-if="store.error"
    class="rounded-xl bg-red-50 border border-red-200 p-4 text-red-700 text-sm"
  >
    <strong>Hata:</strong> {{ store.error }}
  </div>

  <!-- Empty -->
  <div
    v-else-if="store.filteredGames?.length === 0 && store.games?.length > 0"
    class="text-center py-16 text-gray-400 text-sm"
  >
    Seçili liglere ait maç bulunamadı.
  </div>

  <!-- Initial state -->
  <div v-else-if="!store.games?.length" class="text-center py-16 text-gray-300">
    <svg
      xmlns="http://www.w3.org/2000/svg"
      class="h-12 w-12 mx-auto mb-3"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
    >
      <path
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="1.5"
        d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
      />
    </svg>
    <p class="text-sm">Sezon ve ligleri seçip fikstürü yükleyin.</p>
  </div>

  <!-- Games grouped by league -->
  <div v-else class="space-y-6">
    <div
      v-for="(leagueGames, leagueName) in store.gamesByLeague"
      :key="leagueName"
    >
      <h3
        class="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-2 flex items-center gap-2"
      >
        <span class="h-px flex-1 bg-gray-100"></span>
        {{ leagueName }}
        <span class="text-xs font-normal text-gray-400"
          >({{ leagueGames.length }} maç)</span
        >
        <span class="h-px flex-1 bg-gray-100"></span>
      </h3>
      <div class="space-y-2">
        <GameCard v-for="(game, i) in leagueGames" :key="i" :gameData="game" />
      </div>
    </div>
  </div>
</template>
