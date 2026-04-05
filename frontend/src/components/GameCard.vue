<script setup lang="ts">
import type { Game } from '@/types'

defineProps<{ game: Game }>()

function isPlayed(score: string) {
  return score && score.trim() !== ''
}
</script>

<template>
  <div
    :class="[
      'rounded-lg border p-3 transition-shadow hover:shadow-md',
      isPlayed(game.score) ? 'bg-gray-50 border-gray-200' : 'bg-white border-gray-100',
    ]"
  >
    <div class="flex items-center justify-between gap-2">
      <!-- Date / Time -->
      <div class="text-xs text-gray-400 w-20 shrink-0">
        <div class="font-medium text-gray-600">{{ game.date }}</div>
        <div>{{ game.time }}</div>
      </div>

      <!-- Teams -->
      <div class="flex-1 min-w-0">
        <div class="flex items-center justify-between gap-2">
          <span
            :class="[
              'text-sm font-semibold truncate',
              game.homeTeam.toLowerCase().includes('göztepe') ? 'text-goztepe-red' : 'text-gray-800',
            ]"
          >
            {{ game.homeTeam }}
          </span>

          <!-- Score / VS -->
          <div class="shrink-0 text-center min-w-[56px]">
            <span v-if="isPlayed(game.score)" class="text-sm font-bold text-gray-700 tracking-widest">
              {{ game.score }}
            </span>
            <span v-else class="text-xs font-medium text-gray-400 bg-gray-100 rounded px-2 py-0.5">vs</span>
          </div>

          <span
            :class="[
              'text-sm font-semibold truncate text-right',
              game.awayTeam.toLowerCase().includes('göztepe') ? 'text-goztepe-red' : 'text-gray-800',
            ]"
          >
            {{ game.awayTeam }}
          </span>
        </div>

        <div class="mt-1 text-xs text-gray-400 truncate">
          {{ game.venue }}
        </div>
      </div>

      <!-- Group badge -->
      <div class="shrink-0">
        <span class="text-xs bg-gray-100 text-gray-500 rounded px-1.5 py-0.5">
          Gr. {{ game.group }}
        </span>
      </div>
    </div>
  </div>
</template>
