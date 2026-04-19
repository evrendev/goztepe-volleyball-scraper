<script setup lang="ts">
import { computed } from "vue";
import { useFixtureStore } from "@/stores/volleyball/fixture";
import GameCard from "./GameCard.vue";

const store = useFixtureStore();

const hasUpcoming = computed(() => store.upcomingGames.length > 0);
const hasRecent = computed(() => store.recentGames.length > 0);
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

  <!-- No Selection -->
  <div
    v-else-if="!store.selectedDivision"
    class="text-center py-16 text-gray-300"
  >
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
    <p class="text-sm">
      Fikstürü görüntülemek için yukarıdan sezon, kategori, lig ve küme seçimi
      yapın.
    </p>
  </div>

  <!-- No Games -->
  <div
    v-else-if="!store.games?.length"
    class="text-center py-16 text-gray-400 text-sm"
  >
    Bu ligde maç bulunamadı.
  </div>

  <!-- Games Content -->
  <div v-else class="space-y-6">
    <!-- Upcoming Games -->
    <div v-if="hasUpcoming">
      <div class="mb-4">
        <h3 class="text-lg font-semibold text-gray-700 flex items-center gap-2">
          <svg
            class="w-5 h-5 text-green-500"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z"
              clip-rule="evenodd"
            />
          </svg>
          Gelecek Maçlar
          <span class="text-sm font-normal text-gray-400"
            >({{ store.upcomingGames.length }})</span
          >
        </h3>
      </div>

      <div class="space-y-4 mb-8">
        <GameCard
          v-for="(game, index) in store.upcomingGames.slice(0, 8)"
          :key="`upcoming-${index}`"
          :game-data="game"
        />
      </div>
    </div>

    <!-- Recent Games -->
    <div v-if="hasRecent">
      <div class="mb-4">
        <h3 class="text-lg font-semibold text-gray-700 flex items-center gap-2">
          <svg
            class="w-5 h-5 text-blue-500"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path
              fill-rule="evenodd"
              d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z"
              clip-rule="evenodd"
            />
          </svg>
          Oynanan Maçlar
          <span class="text-sm font-normal text-gray-400"
            >({{ store.recentGames.length }})</span
          >
        </h3>
      </div>

      <div class="space-y-4 mb-8">
        <GameCard
          v-for="(game, index) in store.recentGames.slice(0, 8)"
          :key="`recent-${index}`"
          :game-data="game"
        />
      </div>
    </div>

    <!-- All Games -->
    <div
      v-if="
        store.games?.length >
        store.upcomingGames.length + store.recentGames.length
      "
    >
      <div class="mb-4">
        <h3 class="text-lg font-semibold text-gray-700 flex items-center gap-2">
          <svg
            class="w-5 h-5 text-gray-500"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path
              fill-rule="evenodd"
              d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z"
              clip-rule="evenodd"
            />
          </svg>
          Tüm Maçlar
          <span class="text-sm font-normal text-gray-400"
            >({{ store.games.length }})</span
          >
        </h3>
      </div>

      <div class="space-y-3">
        <GameCard v-for="(game, i) in store.games" :key="i" :game-data="game" />
      </div>
    </div>
  </div>
</template>
