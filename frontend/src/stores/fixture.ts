import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { fixtureService } from "@/services/volleyballService";
import type { Game, LeagueDefinition, FixtureRequest } from "@/types";

export const useFixtureStore = defineStore("fixture", () => {
  const leagues = ref<LeagueDefinition[]>([]);
  const games = ref<Game[]>([]);
  const selectedLeagueCodes = ref<string[]>([]);
  const selectedSeason = ref("2025-2026");
  const selectedCategory = ref("");
  const selectedLeagueCode = ref("");
  const selectedDivision = ref("");
  const availableDivisions = ref<{ code: string; name: string }[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const availableSeasons = ["2025-2026", "2024-2025", "2023-2024", "2022-2023"];
  const availableCategories = [
    { code: "GK", name: "Genç Kızlar" },
    { code: "KK", name: "Küçük Kızlar" },
    { code: "YK", name: "Yıldız Kızlar" },
    { code: "MdK", name: "Midi Kızlar" },
    { code: "MnK", name: "Mini Kızlar" },
  ];

  const availableLeagues = computed(() => {
    if (
      !leagues.value ||
      !Array.isArray(leagues.value) ||
      !selectedCategory.value
    )
      return [];
    return leagues.value.filter(
      (l) => l.categoryCode === selectedCategory.value,
    );
  });

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

  const goztepeGames = computed(() => {
    if (!games.value || !Array.isArray(games.value)) return [];
    return games.value.filter(
      (g) =>
        g.homeTeam?.toLowerCase().includes("göztepe") ||
        g.awayTeam?.toLowerCase().includes("göztepe"),
    );
  });

  const upcomingGames = computed(() => {
    if (!games.value || !Array.isArray(games.value)) return [];
    const now = new Date();
    return games.value
      .filter((g) => {
        if (!g.date) return false;
        const gameDate = new Date(g.date.split(".").reverse().join("-"));
        return gameDate > now;
      })
      .sort((a, b) => {
        const dateA = new Date(a.date.split(".").reverse().join("-"));
        const dateB = new Date(b.date.split(".").reverse().join("-"));
        return dateA.getTime() - dateB.getTime();
      });
  });

  const recentGames = computed(() => {
    if (!games.value || !Array.isArray(games.value)) return [];
    const now = new Date();
    return games.value
      .filter((g) => {
        if (!g.date) return false;
        const gameDate = new Date(g.date.split(".").reverse().join("-"));
        return gameDate <= now && g.score;
      })
      .sort((a, b) => {
        const dateA = new Date(a.date.split(".").reverse().join("-"));
        const dateB = new Date(b.date.split(".").reverse().join("-"));
        return dateB.getTime() - dateA.getTime();
      });
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

  async function fetchDivisions() {
    if (
      !selectedSeason.value ||
      !selectedCategory.value ||
      !selectedLeagueCode.value
    ) {
      availableDivisions.value = [];
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const payload: FixtureRequest = {
        seasonId: selectedSeason.value,
        leagues: [selectedLeagueCode.value],
        category: selectedCategory.value,
        // Don't specify division to get all games for this league/category
      };

      const allGames = await fixtureService.getGames(payload, false);
      const divisions = [
        ...new Set(allGames.map((game) => game.division).filter(Boolean)),
      ];
      availableDivisions.value = divisions.map((div) => ({
        code: div,
        name: div,
      }));
    } catch (e) {
      error.value =
        e instanceof Error ? e.message : "Küme/Grup bilgileri yüklenemedi";
      availableDivisions.value = [];
    } finally {
      loading.value = false;
    }
  }

  async function fetchGamesByDivision(forceRefresh = false) {
    if (!selectedLeagueCode.value || !selectedDivision.value) {
      games.value = [];
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const payload: FixtureRequest = {
        seasonId: selectedSeason.value,
        leagues: [selectedLeagueCode.value],
        division: selectedDivision.value,
        category: selectedCategory.value,
      };

      games.value = await fixtureService.getGames(payload, forceRefresh);
    } catch (e) {
      error.value = e instanceof Error ? e.message : "Fikstür yüklenemedi";
      games.value = [];
    } finally {
      loading.value = false;
    }
  }

  function changeSeason(seasonId: string) {
    selectedSeason.value = seasonId;
    selectedLeagueCode.value = "";
    selectedDivision.value = "";
    games.value = [];
    availableDivisions.value = [];
    // Force reload leagues to ensure fresh data
    leagues.value = [];
    fetchLeagues();
  }

  function changeCategory(categoryCode: string) {
    selectedCategory.value = categoryCode;
    selectedLeagueCode.value = "";
    selectedDivision.value = "";
    games.value = [];
    availableDivisions.value = [];
    // Force reload leagues to ensure fresh data
    leagues.value = [];
    fetchLeagues();
  }

  function changeLeague(leagueCode: string) {
    selectedLeagueCode.value = leagueCode;
    selectedDivision.value = "";
    games.value = [];
    // Clear divisions - will be populated by fetchDivisions
    availableDivisions.value = [];
    if (leagueCode && selectedSeason.value && selectedCategory.value) {
      fetchDivisions();
    }
  }

  function changeDivision(division: string) {
    selectedDivision.value = division;
    if (
      selectedLeagueCode.value &&
      selectedCategory.value &&
      selectedSeason.value
    ) {
      fetchGamesByDivision();
    }
  }

  function resetAll() {
    selectedSeason.value = "";
    selectedCategory.value = "";
    selectedLeagueCode.value = "";
    selectedDivision.value = "";
    selectedLeagueCodes.value = [];
    leagues.value = [];
    availableDivisions.value = [];
    games.value = [];
    error.value = null;
  }

  return {
    // State
    leagues,
    games,
    selectedLeagueCodes,
    selectedSeason,
    selectedCategory,
    selectedLeagueCode,
    selectedDivision,
    availableDivisions,
    loading,
    error,

    // Static
    availableSeasons,
    availableCategories,

    // Computed
    availableLeagues,
    filteredGames,
    gamesByLeague,
    goztepeGames,
    upcomingGames,
    recentGames,

    // Actions
    fetchLeagues,
    fetchGames,
    fetchDivisions,
    fetchGamesByDivision,
    toggleLeague,
    selectAllLeagues,
    clearLeagueSelection,
    changeSeason,
    changeCategory,
    changeLeague,
    changeDivision,
    resetAll,
  };
});
