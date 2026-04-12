<script setup lang="ts">
import type { GameResult } from "@/types";
import UnifiedGameCard from "./UnifiedGameCard.vue";

interface Props {
  games: GameResult[];
  loading?: boolean;
}

const { games, loading = false } = defineProps<Props>();
</script>

<template>
  <div
    class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden"
  >
    <div class="px-4 py-3 bg-gray-50 border-b border-gray-100">
      <h3 class="text-sm font-semibold text-gray-700 uppercase tracking-wide">
        Maç Sonuçları
      </h3>
    </div>

    <div v-if="loading" class="p-8 text-center text-gray-400">
      <svg
        class="animate-spin h-6 w-6 mx-auto mb-2"
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
        <path
          class="opacity-75"
          fill="currentColor"
          d="M4 12a8 8 0 018-8v8H4z"
        />
      </svg>
      <p class="text-sm">Maç sonuçları yükleniyor...</p>
    </div>

    <div v-else-if="games.length === 0" class="p-8 text-center text-gray-400">
      <p class="text-sm">Maç sonucu bulunamadı.</p>
    </div>

    <div v-else class="space-y-3 p-4">
      <UnifiedGameCard
        v-for="game in games"
        :key="game.rowNo"
        :game-data="game"
      />
    </div>
  </div>
</template>
