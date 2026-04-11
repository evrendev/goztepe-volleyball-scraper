<script setup lang="ts">
import type { StandingsRow } from "@/types";

interface Props {
  standings: StandingsRow[];
  loading?: boolean;
}

const { standings, loading = false } = defineProps<Props>();
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
            :key="team.position"
            class="border-b border-gray-50 hover:bg-gray-25"
          >
            <td class="py-3 px-4">
              <span class="text-sm font-medium">{{ team.position }}</span>
            </td>
            <td class="py-3 px-4">
              <span class="text-gray-900">{{ team.teamName }}</span>
            </td>
            <td class="py-3 px-4 text-center text-gray-600">
              {{ team.played }}
            </td>
            <td class="py-3 px-4 text-center text-gray-600">{{ team.won }}</td>
            <td class="py-3 px-4 text-center text-gray-600">{{ team.lost }}</td>
            <td class="py-3 px-4 text-center text-gray-600">{{ team.sets }}</td>
            <td class="py-3 px-4 text-center">
              <span class="font-medium text-gray-900">{{ team.points }}</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
