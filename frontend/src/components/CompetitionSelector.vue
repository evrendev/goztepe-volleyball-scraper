<script setup lang="ts">
import { useStandingsStore } from "@/stores/standings";
import type { Competition } from "@/types";

const store = useStandingsStore();
</script>

<template>
  <div
    class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 space-y-4"
  >
    <!-- Season, Category, League Selectors -->
    <div class="flex flex-wrap items-end gap-4">
      <!-- Season -->
      <div class="flex flex-col gap-1">
        <label
          class="text-xs font-semibold text-gray-500 uppercase tracking-wide"
          >Sezon</label
        >
        <select
          :value="store.selectedSeason"
          @change="
            store.changeSeason(($event.target as HTMLSelectElement).value)
          "
          class="border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-goztepe-red focus:border-transparent"
        >
          <option
            v-for="season in store.availableSeasons"
            :key="season"
            :value="season"
          >
            {{ season }}
          </option>
        </select>
      </div>

      <!-- Category -->
      <div class="flex flex-col gap-1">
        <label
          class="text-xs font-semibold text-gray-500 uppercase tracking-wide"
          >Kategori</label
        >
        <select
          :value="store.selectedCategory"
          @change="
            store.changeCategory(($event.target as HTMLSelectElement).value)
          "
          class="border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-goztepe-red focus:border-transparent"
        >
          <option
            v-for="category in store.availableCategories"
            :key="category.code"
            :value="category.code"
          >
            {{ category.name }}
          </option>
        </select>
      </div>

      <!-- League -->
      <div class="flex flex-col gap-1">
        <label
          class="text-xs font-semibold text-gray-500 uppercase tracking-wide"
          >Lig</label
        >
        <select
          :value="store.selectedLeagueCode"
          @change="
            store.changeLeague(($event.target as HTMLSelectElement).value)
          "
          class="border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-goztepe-red focus:border-transparent"
        >
          <option
            v-for="league in store.availableLeagues"
            :key="league.code"
            :value="league.code"
          >
            {{ league.name }}
          </option>
        </select>
      </div>

      <!-- Refresh Button -->
      <button
        @click="store.fetchCompetitions()"
        :disabled="store.loading"
        class="flex items-center gap-2 px-4 py-2 bg-goztepe-red text-white text-sm font-medium rounded-lg hover:bg-goztepe-dark focus:outline-none focus:ring-2 focus:ring-goztepe-red focus:ring-offset-2 disabled:opacity-50"
      >
        <svg
          class="w-4 h-4"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
          />
        </svg>
        Yarışmaları Yükle
      </button>
    </div>

    <!-- Competition Selector -->
    <div v-if="store.competitions.length > 0" class="space-y-2">
      <label class="text-sm font-semibold text-gray-700">Yarışma Seçin</label>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
        <button
          v-for="competition in store.competitions"
          :key="competition.name"
          @click="store.selectCompetition(competition)"
          :class="[
            'text-left p-3 rounded-lg border transition-colors text-sm',
            store.selectedCompetition?.name === competition.name
              ? 'border-goztepe-red bg-goztepe-red bg-opacity-5 text-goztepe-dark'
              : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50',
          ]"
        >
          <div class="font-medium">{{ competition.displayName }}</div>
          <div class="text-xs text-gray-500 mt-1">
            {{ competition.leagueCode || competition.category }}
            <span v-if="competition.hasGoztepe" class="ml-1 text-goztepe-red"
              >★ Göztepe</span
            >
          </div>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div
      v-if="store.loading && store.competitions.length === 0"
      class="text-center py-4"
    >
      <svg
        class="animate-spin h-6 w-6 mx-auto mb-2 text-goztepe-red"
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
      <p class="text-sm text-gray-400">Yarışmalar yükleniyor...</p>
    </div>

    <!-- No Competitions -->
    <div
      v-if="!store.loading && store.competitions.length === 0"
      class="text-center py-8 text-gray-400"
    >
      <svg
        class="h-8 w-8 mx-auto mb-2"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="1"
          d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
        />
      </svg>
      <p class="text-sm">Bu kategoride yarışma bulunamadı.</p>
      <p class="text-xs mt-1">Farklı kategori veya sezon deneyin.</p>
    </div>

    <!-- Error State -->
    <div
      v-if="store.error"
      class="bg-red-50 border border-red-200 rounded-lg p-3"
    >
      <div class="flex">
        <svg
          class="w-5 h-5 text-red-400"
          fill="currentColor"
          viewBox="0 0 20 20"
        >
          <path
            fill-rule="evenodd"
            d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
            clip-rule="evenodd"
          />
        </svg>
        <div class="ml-3">
          <h3 class="text-sm font-medium text-red-800">Hata oluştu</h3>
          <div class="mt-1 text-sm text-red-700">{{ store.error }}</div>
        </div>
      </div>
    </div>
  </div>
</template>
