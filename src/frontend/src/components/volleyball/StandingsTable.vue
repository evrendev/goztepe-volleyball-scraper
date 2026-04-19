<script setup lang="ts">
import type { StandingsRow } from "@/types/volleyball";

interface Props {
  standings: StandingsRow[];
  loading?: boolean;
}

const { standings, loading = false } = defineProps<Props>();

function formatSetRatio(setsWon: number, setsLost: number): string {
  return `${setsWon}-${setsLost}`;
}
</script>

<template>
  <div
    class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden"
  >
    <div class="px-4 py-3 bg-gray-50 border-b border-gray-100">
      <h3 class="text-sm font-semibold text-gray-700 uppercase tracking-wide">
        Puan Durumu
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
      <p class="text-sm">Puan durumu yükleniyor...</p>
    </div>

    <div
      v-else-if="standings.length === 0"
      class="p-8 text-center text-gray-400"
    >
      <p class="text-sm">Puan durumu bulunamadı.</p>
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="bg-gray-50 border-b border-gray-100">
            <th class="text-left py-3 px-4 font-medium text-gray-600">#</th>
            <th class="text-left py-3 px-4 font-medium text-gray-600">Takım</th>
            <th class="text-center py-3 px-4 font-medium text-gray-600">O</th>
            <th class="text-center py-3 px-4 font-medium text-gray-600">G</th>
            <th class="text-center py-3 px-4 font-medium text-gray-600">M</th>
            <th class="text-center py-3 px-4 font-medium text-gray-600">Set</th>
            <th class="text-center py-3 px-4 font-medium text-gray-600">
              Puan
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="team in standings"
            :key="team.rank"
            :class="[
              'border-b border-gray-50 hover:bg-gray-25 transition-colors',
              team.isGoztepe
                ? 'bg-goztepe-red hover:bg-red-600 text-white'
                : '',
            ]"
          >
            <td class="py-3 px-4">
              <span
                :class="[
                  'text-sm font-medium',
                  team.isGoztepe ? 'text-white' : 'text-gray-900',
                ]"
              >
                {{ team.rank }}
              </span>
            </td>
            <td class="py-3 px-4">
              <div class="flex items-center gap-3">
                <img
                  v-if="team.logoUrl"
                  :src="team.logoUrl"
                  :alt="team.teamName + ' logo'"
                  class="w-6 h-6 rounded-full object-contain flex-shrink-0"
                  @error="
                    ($event.target as HTMLImageElement).style.display = 'none'
                  "
                />
                <span
                  :class="[
                    'font-medium',
                    team.isGoztepe ? 'text-white font-bold' : 'text-gray-900',
                  ]"
                >
                  {{ team.teamName }}
                </span>
              </div>
            </td>
            <td
              class="py-3 px-4 text-center"
              :class="team.isGoztepe ? 'text-white' : 'text-gray-600'"
            >
              {{ team.played }}
            </td>
            <td
              class="py-3 px-4 text-center"
              :class="team.isGoztepe ? 'text-white' : 'text-gray-600'"
            >
              {{ team.won }}
            </td>
            <td
              class="py-3 px-4 text-center"
              :class="team.isGoztepe ? 'text-white' : 'text-gray-600'"
            >
              {{ team.lost }}
            </td>
            <td
              class="py-3 px-4 text-center"
              :class="team.isGoztepe ? 'text-white' : 'text-gray-600'"
            >
              {{ formatSetRatio(team.setsWon, team.setsLost) }}
            </td>
            <td class="py-3 px-4 text-center">
              <span
                :class="[
                  'font-medium',
                  team.isGoztepe ? 'text-white font-bold' : 'text-gray-900',
                ]"
              >
                {{ team.points }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
