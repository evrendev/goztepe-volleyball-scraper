<script setup lang="ts">
import { useVolleyballStore } from '@/stores/volleyball'

const store = useVolleyballStore()
</script>

<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
    <div class="flex items-center justify-between mb-3">
      <h2 class="text-sm font-semibold text-gray-700 uppercase tracking-wide">Ligler</h2>
      <div class="flex gap-2">
        <button
          class="text-xs text-goztepe-red hover:underline"
          @click="store.selectAllLeagues"
        >
          Tümü
        </button>
        <span class="text-gray-300">|</span>
        <button
          class="text-xs text-gray-400 hover:underline"
          @click="store.clearLeagueSelection"
        >
          Temizle
        </button>
      </div>
    </div>
    <div class="flex flex-wrap gap-2">
      <button
        v-for="league in store.leagues"
        :key="league.code"
        :class="[
          'px-3 py-1.5 rounded-full text-sm font-medium transition-all',
          store.selectedLeagueCodes.includes(league.code)
            ? 'bg-goztepe-red text-white shadow-sm'
            : 'bg-gray-100 text-gray-600 hover:bg-gray-200',
        ]"
        @click="store.toggleLeague(league.code)"
      >
        {{ league.code }}
      </button>
    </div>
  </div>
</template>
