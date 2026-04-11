import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { fixtureService } from "@/services/volleyballService";
import type { Game, LeagueDefinition } from "@/types";

export const useFixtureStore = defineStore("fixture", () => {
  const leagues = ref<LeagueDefinition[]>([]);
  const games = ref<Game[]>([]);
  const selectedLeagueCodes = ref<string[]>([]);
  const selectedSeason = ref("2025-2026");
  const loading = ref(false);
  const error = ref<string | null>(null);

  const availableSeasons = ["2025-2026", "2024-2025", "2023-2024", "2022-2023"];

  const filteredGames = computed(() => {
    if (!games.value || !Array.isArray(games.value)) return [];
    if (selectedLeagueCodes.value.length === 0) return games.value;
    return games.value.filter((g) =>
      selectedLeagueCodes.value.includes(g.division),
    );
  });

  const gamesByLeague = computed(() => {
    const grouped: Record<string, Game[]> = {};
    if (!filteredGames.value || !Array.isArray(filteredGames.value))
      return grouped;
    for (const game of filteredGames.value) {
      const key = game.league || game.division;
      if (!grouped[key]) grouped[key] = [];
      grouped[key].push(game);
    }
    return grouped;
  });

  async function fetchLeagues() {
    try {
      leagues.value = await fixtureService.getLeagues();
    } catch (e) {
      error.value = e instanceof Error ? e.message : "Ligler yüklenemedi";
    }
  }

  async function fetchGames(forceRefresh = false) {
    loading.value = true;
    error.value = null;
    try {
      games.value = await fixtureService.getGames(
        {
          seasonId: selectedSeason.value,
          leagues: selectedLeagueCodes.value.length
            ? selectedLeagueCodes.value
            : undefined,
        },
        forceRefresh,
      );
    } catch (e) {
      error.value = e instanceof Error ? e.message : "Fikstür yüklenemedi";
    } finally {
      loading.value = false;
    }
  }

  function toggleLeague(code: string) {
    const idx = selectedLeagueCodes.value.indexOf(code);
    if (idx === -1) {
      selectedLeagueCodes.value.push(code);
    } else {
      selectedLeagueCodes.value.splice(idx, 1);
    }
  }

  function selectAllLeagues() {
    selectedLeagueCodes.value = leagues.value.map((l) => l.code);
  }

  function clearLeagueSelection() {
    selectedLeagueCodes.value = [];
  }

  return {
    leagues,
    games,
    selectedLeagueCodes,
    selectedSeason,
    loading,
    error,
    availableSeasons,
    filteredGames,
    gamesByLeague,
    fetchLeagues,
    fetchGames,
    toggleLeague,
    selectAllLeagues,
    clearLeagueSelection,
  };
});
