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

function getResultBadgeClass(result: string) {
  switch (result?.toLowerCase()) {
    case "g":
    case "galibiyet":
      return "bg-green-100 text-green-800";
    case "m":
    case "mağlubiyet":
      return "bg-red-100 text-red-800";
    default:
      return "bg-gray-100 text-gray-800";
  }
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

    <div v-else class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="bg-gray-50 border-b border-gray-100">
            <th class="text-left py-3 px-4 font-medium text-gray-600">Tarih</th>
            <th class="text-left py-3 px-4 font-medium text-gray-600">Saat</th>
            <th class="text-left py-3 px-4 font-medium text-gray-600">
              Ev Sahibi
            </th>
            <th class="text-center py-3 px-4 font-medium text-gray-600">
              Skor
            </th>
            <th class="text-left py-3 px-4 font-medium text-gray-600">
              Deplasman
            </th>
            <th class="text-center py-3 px-4 font-medium text-gray-600">
              Sonuç
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="game in games"
            :key="game.rowNo"
            :class="[
              'border-b border-gray-50 hover:bg-gray-25',
              game.isGoztepeHome || game.isGoztepeAway
                ? 'bg-goztepe-red bg-opacity-5'
                : '',
            ]"
          >
            <td class="py-3 px-4 text-gray-600">
              {{ formatDate(game.date) }}
            </td>
            <td class="py-3 px-4 text-gray-600">
              {{ game.time || "-" }}
            </td>
            <td class="py-3 px-4">
              <span
                :class="
                  game.isGoztepeHome
                    ? 'font-semibold text-goztepe-red'
                    : 'text-gray-900'
                "
              >
                {{ game.homeTeam }}
              </span>
            </td>
            <td class="py-3 px-4 text-center">
              <span class="font-mono text-sm bg-gray-100 px-2 py-1 rounded">
                {{ game.score || "-" }}
              </span>
            </td>
            <td class="py-3 px-4">
              <span
                :class="
                  game.isGoztepeAway
                    ? 'font-semibold text-goztepe-red'
                    : 'text-gray-900'
                "
              >
                {{ game.awayTeam }}
              </span>
            </td>
            <td class="py-3 px-4 text-center">
              <span
                v-if="game.result"
                :class="[
                  'inline-block px-2 py-1 text-xs font-medium rounded-full',
                  getResultBadgeClass(game.result),
                ]"
              >
                {{ game.result }}
              </span>
              <span v-else class="text-gray-400 text-xs">-</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
