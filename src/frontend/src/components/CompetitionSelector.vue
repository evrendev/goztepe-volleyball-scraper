<script setup lang="ts">
import { onMounted, watch } from "vue";
import { useStandingsStore } from "@/stores/standings";

const store = useStandingsStore();

function selectCompetition(competitionName: string) {
  const competition = store.competitions.find(
    (c) => c.name === competitionName,
  );
  if (competition) {
    store.selectCompetition(competition);
  }
}

onMounted(() => {
  // Load leagues for both pages to use
  store.fetchLeagues();

  // Watch for category changes and reload leagues
  watch(
    () => store.selectedCategory,
    (newCategory) => {
      if (newCategory) {
        store.fetchLeagues();
      }
    },
  );

  // Watch for league changes and automatically load competitions
  watch(
    () => store.selectedLeagueCode,
    (newLeagueCode) => {
      if (newLeagueCode && store.selectedSeason && store.selectedCategory) {
        store.fetchCompetitions();
      }
    },
  );
});
</script>

<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
    <!-- Header -->
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-sm font-semibold text-gray-700 uppercase tracking-wide">
        Puan Durumu Seçimi
      </h2>
      <button
        @click="store.resetAll()"
        class="flex items-center gap-1 text-xs text-red-500 hover:text-red-700"
      >
        <svg
          class="w-3 h-3"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
          />
        </svg>
        Sıfırla
      </button>
    </div>

    <!-- Selection Controls -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
      <!-- Season -->
      <div>
        <label class="block text-xs font-medium text-gray-600 mb-2"
          >Sezon</label
        >
        <select
          :value="store.selectedSeason"
          @change="
            store.changeSeason(($event.target as HTMLSelectElement).value)
          "
          class="w-full rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-goztepe-red disabled:opacity-50"
        >
          <option value="">Sezon Seçiniz</option>
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
      <div>
        <label class="block text-xs font-medium text-gray-600 mb-2"
          >Kategori</label
        >
        <select
          :value="store.selectedCategory"
          @change="
            store.changeCategory(($event.target as HTMLSelectElement).value)
          "
          :disabled="!store.selectedSeason"
          class="w-full rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-goztepe-red disabled:opacity-50"
        >
          <option value="">Kategori Seçiniz</option>
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
      <div>
        <label class="block text-xs font-medium text-gray-600 mb-2">Lig</label>
        <select
          :value="store.selectedLeagueCode"
          @change="
            store.changeLeague(($event.target as HTMLSelectElement).value)
          "
          :disabled="!store.selectedCategory || !store.availableLeagues.length"
          class="w-full rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-goztepe-red disabled:opacity-50"
        >
          <option value="">Lig Seçiniz</option>
          <option
            v-for="league in store.availableLeagues"
            :key="league.code"
            :value="league.code"
          >
            {{ league.displayName }}
          </option>
        </select>
      </div>

      <!-- Competition -->
      <div>
        <label class="block text-xs font-medium text-gray-600 mb-2"
          >Yarışma</label
        >
        <select
          :value="store.selectedCompetition?.name || ''"
          @change="
            selectCompetition(($event.target as HTMLSelectElement).value)
          "
          :disabled="!store.selectedLeagueCode || !store.competitions.length"
          class="w-full rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-goztepe-red disabled:opacity-50"
        >
          <option value="">Yarışma Seçiniz</option>
          <option
            v-for="competition in store.competitions"
            :key="competition.name"
            :value="competition.name"
          >
            {{ competition.displayName }}
          </option>
        </select>
      </div>
    </div>

    <!-- Loading State -->
    <div
      v-if="store.loading"
      class="flex items-center gap-2 p-3 text-sm text-gray-500"
    >
      <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
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
      Yarışmalar yükleniyor...
    </div>

    <!-- Error State -->
    <div
      v-else-if="store.error"
      class="p-3 text-sm text-red-600 bg-red-50 rounded-lg"
    >
      {{ store.error }}
    </div>
  </div>
</template>
