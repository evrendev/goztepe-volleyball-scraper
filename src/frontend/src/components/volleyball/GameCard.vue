<script setup lang="ts">
import type { Game, GameResult } from "@/types/volleyball";

interface Props {
  gameData: Game | GameResult;
}

const { gameData } = defineProps<Props>();

function isPlayed(data: Game | GameResult): boolean {
  if (isGameResult(data)) {
    return data.isPlayed;
  }
  return Boolean(data.score && data.score.trim() !== "");
}

function isGameResult(data: Game | GameResult): data is GameResult {
  return "homeScore" in data || "isPlayed" in data;
}

function isGoztepeTeam(teamName: string, data: Game | GameResult): boolean {
  if (isGameResult(data)) {
    // For GameResult, check if it's a Göztepe game
    return (
      data.isGoztepe &&
      (teamName === data.homeTeam || teamName === data.awayTeam)
    );
  }
  // Game case - check team name
  return teamName.toLowerCase().includes("göztepe");
}

function hasGoztepeInGame(data: Game | GameResult): boolean {
  if (isGameResult(data)) {
    return data.isGoztepe;
  }
  return (
    data.homeTeam.toLowerCase().includes("göztepe") ||
    data.awayTeam.toLowerCase().includes("göztepe")
  );
}

function getScoreDisplay(data: Game | GameResult): string {
  if (isGameResult(data)) {
    if (data.isPlayed) {
      return `${data.homeScore}-${data.awayScore}`;
    }
    return "vs";
  }
  return data.score || "vs";
}

function getGroupInfo(data: Game | GameResult): string | null {
  if ("group" in data) {
    return data.group;
  }
  return null;
}

function getVenueInfo(data: Game | GameResult): string | null {
  if ("venue" in data) {
    return data.venue;
  }
  return null;
}

function getSetResults(data: Game | GameResult): string | null {
  if (isGameResult(data) && data.isPlayed) {
    return data.setResults;
  }
  return null;
}

function formatDateToTurkish(dateStr: string): string {
  if (!dateStr) return "";

  // Convert DD.MM.YYYY to Turkish format
  const [day, month, year] = dateStr.split(".");
  const monthNames = [
    "",
    "Ocak",
    "Şubat",
    "Mart",
    "Nisan",
    "Mayıs",
    "Haziran",
    "Temmuz",
    "Ağustos",
    "Eylül",
    "Ekim",
    "Kasım",
    "Aralık",
  ];

  const monthName = monthNames[parseInt(month, 10)];
  return `${parseInt(day, 10)} ${monthName} ${year}`;
}
</script>

<template>
  <div
    :class="[
      'rounded-lg border p-3 transition-shadow hover:shadow-md',
      isPlayed(gameData)
        ? 'bg-gray-50 border-gray-200'
        : 'bg-white border-gray-100',
      hasGoztepeInGame(gameData) ? 'border-l-4 border-l-goztepe-red' : '',
    ]"
  >
    <div class="flex items-center justify-between gap-4">
      <!-- Far Left: Home Team -->
      <div class="flex-1 text-left">
        <div
          :class="[
            'text-sm font-semibold',
            isGoztepeTeam(gameData.homeTeam, gameData)
              ? 'text-goztepe-red font-bold'
              : 'text-gray-800',
          ]"
        >
          {{ gameData.homeTeam }}
        </div>
        <div v-if="getGroupInfo(gameData)" class="text-xs text-gray-500 mt-1">
          Grup {{ getGroupInfo(gameData) }}
        </div>
      </div>

      <!-- Center: Date, Score, Time -->
      <div class="text-center space-y-1 min-w-20">
        <!-- Date above -->
        <div class="text-xs text-gray-600 font-medium">
          {{ formatDateToTurkish(gameData.date) }}
        </div>

        <!-- Time below date -->
        <div
          v-if="gameData.time && gameData.time.trim()"
          class="text-xs text-gray-500"
        >
          {{ gameData.time }}
        </div>

        <!-- Debug: Show when time is missing -->
        <div v-else-if="!isPlayed(gameData)" class="text-xs text-red-400">
          -
        </div>

        <!-- Score -->
        <div class="px-2">
          <span
            v-if="isPlayed(gameData)"
            class="text-xs font-bold text-gray-700 tracking-widest bg-gray-200 px-2 py-1 rounded"
          >
            {{ getScoreDisplay(gameData) }}
          </span>
        </div>

        <!-- Set results below (only for played games) -->
        <div v-if="getSetResults(gameData)" class="text-xs text-gray-400">
          {{ getSetResults(gameData) }}
        </div>
      </div>

      <!-- Far Right: Away Team -->
      <div class="flex-1 text-right">
        <div
          :class="[
            'text-sm font-semibold',
            isGoztepeTeam(gameData.awayTeam, gameData)
              ? 'text-goztepe-red font-bold'
              : 'text-gray-800',
          ]"
        >
          {{ gameData.awayTeam }}
        </div>
        <div
          v-if="getVenueInfo(gameData)"
          class="text-xs text-gray-500 mt-1 truncate"
        >
          {{ getVenueInfo(gameData) }}
        </div>
      </div>
    </div>
  </div>
</template>
