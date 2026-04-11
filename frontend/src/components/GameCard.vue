<script setup lang="ts">
import type { Game } from "@/types";

defineProps<{ game: Game }>();

function isPlayed(score: string) {
  return score && score.trim() !== "";
}
</script>

<template>
  <div
    :class="[
      'rounded-lg border p-3 transition-shadow hover:shadow-md',
      isPlayed(game.score)
        ? 'bg-gray-50 border-gray-200'
        : 'bg-white border-gray-100',
      game.homeTeam.toLowerCase().includes('göztepe') ||
      game.awayTeam.toLowerCase().includes('göztepe')
        ? 'border-l-4 border-l-goztepe-red'
        : '',
    ]"
  >
    <div class="flex items-center justify-between gap-4">
      <!-- Far Left: Home Team -->
      <div class="flex-1 text-left">
        <div
          :class="[
            'text-sm font-semibold',
            game.homeTeam.toLowerCase().includes('göztepe')
              ? 'text-goztepe-red font-bold'
              : 'text-gray-800',
          ]"
        >
          {{ game.homeTeam }}
        </div>
        <div v-if="game.group" class="text-xs text-gray-500 mt-1">
          Grup {{ game.group }}
        </div>
      </div>

      <!-- Center: Date, VS/Score, Time -->
      <div class="text-center space-y-1 min-w-20">
        <!-- Date above -->
        <div class="text-xs text-gray-600 font-medium">{{ game.date }}</div>

        <!-- VS/Score -->
        <div class="px-2">
          <span
            v-if="isPlayed(game.score)"
            class="text-xs font-bold text-gray-700 tracking-widest bg-gray-200 px-2 py-1 rounded"
          >
            {{ game.score }}
          </span>
          <span
            v-else
            class="text-xs font-medium text-gray-400 bg-gray-100 rounded px-2 py-0.5"
          >
            vs
          </span>
        </div>

        <!-- Time below -->
        <div class="text-xs text-gray-400">{{ game.time }}</div>
      </div>

      <!-- Far Right: Away Team -->
      <div class="flex-1 text-right">
        <div
          :class="[
            'text-sm font-semibold',
            game.awayTeam.toLowerCase().includes('göztepe')
              ? 'text-goztepe-red font-bold'
              : 'text-gray-800',
          ]"
        >
          {{ game.awayTeam }}
        </div>
        <div v-if="game.venue" class="text-xs text-gray-500 mt-1 truncate">
          {{ game.venue }}
        </div>
      </div>
    </div>
  </div>
</template>
