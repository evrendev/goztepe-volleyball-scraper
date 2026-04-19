<script setup lang="ts">
import { onMounted } from "vue";
import { useFixtureStore } from "@/stores/volleyball/fixture";
import LeagueFilter from "@/components/volleyball/LeagueFilter.vue";
import GameList from "@/components/volleyball/GameList.vue";
import CacheManager from "@/components/volleyball/CacheManager.vue";

const store = useFixtureStore();

onMounted(() => {
  store.fetchLeagues();
});
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-goztepe-dark text-white shadow-lg">
      <div
        class="max-w-4xl mx-auto px-4 py-4 flex items-center justify-between"
      >
        <div class="flex items-center gap-3">
          <div
            class="w-10 h-10 bg-goztepe-red rounded-full flex items-center justify-center shrink-0"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-6 w-6"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <circle cx="12" cy="12" r="10" stroke-width="1.5" />
              <path
                stroke-linecap="round"
                stroke-width="1.5"
                d="M12 2C6.5 6 6.5 18 12 22M12 2c5.5 4 5.5 16 0 20M2 12h20"
              />
            </svg>
          </div>
          <div>
            <h1 class="text-lg font-bold leading-tight">Göztepe Voleybol</h1>
            <p class="text-xs text-gray-400">İzmir İl Voleybol Fikstürleri</p>
          </div>
        </div>

        <!-- Navigation Links -->
        <nav class="flex space-x-4">
          <router-link
            :to="{ name: 'volleyball-fixture' }"
            class="px-3 py-2 text-sm font-medium rounded-md bg-goztepe-red text-white"
          >
            Fikstür
          </router-link>
          <router-link
            :to="{ name: 'volleyball-standings' }"
            class="px-3 py-2 text-sm font-medium rounded-md text-gray-300 hover:text-white hover:bg-goztepe-red transition-colors"
          >
            Puan Durumu
          </router-link>
        </nav>
      </div>
    </header>

    <main class="max-w-4xl mx-auto px-4 py-6 space-y-4">
      <!-- Controls -->
      <div
        class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 flex flex-wrap items-end gap-4"
      >
        <!-- Season selector -->
        <div class="flex flex-col gap-1">
          <label
            class="text-xs font-semibold text-gray-500 uppercase tracking-wide"
            >Sezon</label
          >
          <select
            v-model="store.selectedSeason"
            class="rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-goztepe-red"
          >
            <option v-for="s in store.availableSeasons" :key="s" :value="s">
              {{ s }}
            </option>
          </select>
        </div>

        <!-- Load button -->
        <button
          :disabled="store.loading"
          class="ml-auto flex items-center gap-2 bg-goztepe-red hover:bg-red-700 disabled:opacity-50 text-white font-medium text-sm rounded-lg px-5 py-2 transition-colors"
          @click="store.fetchGames()"
        >
          <svg
            v-if="store.loading"
            class="animate-spin h-4 w-4"
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
          Fikstürü Yükle
        </button>

        <!-- Force refresh -->
        <button
          v-if="store.games.length > 0"
          :disabled="store.loading"
          class="flex items-center gap-1.5 text-xs text-gray-400 hover:text-gray-600 disabled:opacity-40 transition-colors"
          @click="store.fetchGames(true)"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="h-3.5 w-3.5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
            />
          </svg>
          Yenile
        </button>
      </div>

      <!-- League filter -->
      <league-filter v-if="store.leagues.length > 0" />

      <!-- Summary -->
      <div
        v-if="store.games?.length > 0"
        class="flex items-center gap-2 text-xs text-gray-400 px-1"
      >
        <span
          >Toplam
          <strong class="text-gray-600">{{
            store.filteredGames?.length || 0
          }}</strong>
          maç</span
        >
        <span v-if="store.selectedLeagueCodes?.length">
          ·
          <strong class="text-gray-600">{{
            store.selectedLeagueCodes?.length || 0
          }}</strong>
          lig seçili
        </span>
      </div>

      <!-- Game list -->
      <game-list />
    </main>

    <!-- Cache Manager -->
    <cache-manager />
  </div>
</template>
