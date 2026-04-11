<script setup lang="ts">
import type { GameResult } from "@/types";

interface Props {
  games: GameResult[];
  loading?: boolean;
}

const { games, loading = false } = defineProps<Props>();

function formatDate(dateStr: string) {
  if (!dateStr) return "";
  // Assume the date is in DD.MM.YYYY format from backend
  const [day, month, year] = dateStr.split(".");
  if (day && month && year) {
    return `${day}/${month}/${year}`;
  }
  return dateStr;
}
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
      <div
        v-for="game in games"
        :key="game.rowNo"
        :class="[
          'rounded-lg border p-3 transition-shadow hover:shadow-md',
          'bg-white border-gray-100',
          game.isGoztepeHome || game.isGoztepeAway ? 'border-l-4 border-l-goztepe-red' : ''
        ]"
      >
        <div class="flex items-center justify-between gap-4">
          <!-- Far Left: Home Team -->
          <div class="flex-1 text-left">
            <div
              :class="[
                'text-sm font-semibold',
                game.isGoztepeHome ? 'text-goztepe-red font-bold' : 'text-gray-800',
              ]"
            >
              {{ game.homeTeam }}
            </div>
          </div>

          <!-- Center: Date, Score, Time -->
          <div class="text-center space-y-1 min-w-20">
            <!-- Date above -->
            <div class="text-xs text-gray-600 font-medium">{{ formatDate(game.date) }}</div>
            
            <!-- Score -->
            <div class="px-2">
              <span class="text-xs font-bold text-gray-700 tracking-widest bg-gray-200 px-2 py-1 rounded">
                {{ game.score || "-" }}
              </span>
            </div>
            
            <!-- Time below -->
            <div class="text-xs text-gray-400">{{ game.time || "-" }}</div>
          </div>

          <!-- Far Right: Away Team -->
          <div class="flex-1 text-right">
            <div
              :class="[
                'text-sm font-semibold',
                game.isGoztepeAway ? 'text-goztepe-red font-bold' : 'text-gray-800',
              ]"
            >
              {{ game.awayTeam }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
