import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { fixtureService, standingsService } from "@/services/volleyballService";
import type {
  Game,
  LeagueDefinition,
  FixtureRequest,
  Competition,
  CompetitionRequest,
} from "@/types";

export const useVolleyballStore = defineStore("volleyball", () => {
  // Data
  const leagues = ref<LeagueDefinition[]>([]);
  const games = ref<Game[]>([]);
  const competitions = ref<Competition[]>([]);
  const availableDivisions = ref<{ code: string; name: string }[]>([]);

  // Selections
  const selectedLeague = ref<LeagueDefinition | null>(null);
  const selectedCompetition = ref<Competition | null>(null);
  const selectedSeason = ref("");
  const selectedCategory = ref("");
  const selectedLeagueCode = ref(""); // Lig kodu
  const selectedDivision = ref(""); // Küme/Grup

  const loading = ref(false);
  const error = ref<string | null>(null);

  // Static data
  const availableSeasons = ["2025-2026", "2024-2025", "2023-2024", "2022-2023"];
  const availableCategories = [
    { code: "GK", name: "Genç Kızlar" },
    { code: "KK", name: "Küçük Kızlar" },
    { code: "YK", name: "Yıldız Kızlar" },
    { code: "MdK", name: "Midi Kızlar" },
    { code: "MnK", name: "Mini Kızlar" },
  ];

  // Computed properties
  const filteredLeagues = computed(() => {
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

  // Game computeds
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

  // Actions
  async function fetchLeagues() {
    try {
      leagues.value = await fixtureService.getLeagues();
    } catch (e) {
      error.value = e instanceof Error ? e.message : "Ligler yüklenemedi";
    }
  }

  async function fetchCompetitions() {
    if (
      !selectedSeason.value ||
      !selectedCategory.value ||
      !selectedLeagueCode.value
    ) {
      competitions.value = [];
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const payload: CompetitionRequest = {
        seasonId: selectedSeason.value,
        category: selectedCategory.value,
        leagueCode: selectedLeagueCode.value,
      };

      const result = await standingsService.getCompetitions(payload);
      competitions.value = result.competitions || [];
    } catch (e) {
      error.value = e instanceof Error ? e.message : "Yarışmalar yüklenemedi";
      competitions.value = [];
    } finally {
      loading.value = false;
    }
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

  async function fetchGames(forceRefresh = false) {
    if (!selectedCompetition.value) {
      games.value = [];
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      const leagueCode =
        selectedCompetition.value.leagueCode ||
        selectedCompetition.value.league;
      const payload: FixtureRequest = {
        seasonId: selectedSeason.value,
        leagues: [leagueCode],
        division: selectedCompetition.value.division,
        category: selectedCompetition.value.category,
        group: selectedCompetition.value.group,
      };

      games.value = await fixtureService.getGames(payload, forceRefresh);
    } catch (e) {
      error.value = e instanceof Error ? e.message : "Fikstür yüklenemedi";
      games.value = [];
    } finally {
      loading.value = false;
    }
  }

  // Selection handlers
  function changeSeason(seasonId: string) {
    selectedSeason.value = seasonId;
    selectedCompetition.value = null;
    selectedLeagueCode.value = "";
    selectedDivision.value = "";
    games.value = [];
    competitions.value = [];
    // Force reload leagues to ensure fresh data
    leagues.value = [];
    fetchLeagues();
  }

  function changeCategory(categoryCode: string) {
    selectedCategory.value = categoryCode;
    selectedCompetition.value = null;
    selectedLeagueCode.value = "";
    selectedDivision.value = "";
    games.value = [];
    competitions.value = [];
    // Force reload leagues to ensure fresh data
    leagues.value = [];
    fetchLeagues();
  }

  function changeLeague(leagueCode: string) {
    selectedLeagueCode.value = leagueCode;
    selectedCompetition.value = null;
    selectedDivision.value = "";
    games.value = [];
    // Clear divisions - will be populated by watch listener
    availableDivisions.value = [];
  }

  function changeDivision(division: string) {
    selectedDivision.value = division;

    // For fixture mode, create a virtual competition object and fetch games directly
    if (
      selectedLeagueCode.value &&
      selectedCategory.value &&
      selectedSeason.value
    ) {
      selectedCompetition.value = {
        name: division,
        displayName: `${selectedLeagueCode.value} - ${division}`,
        league: selectedLeagueCode.value,
        leagueCode: selectedLeagueCode.value,
        category: selectedCategory.value,
        division: division,
        hasGoztepe: false, // Will be determined after fetching games
      };
      fetchGames();
    }
  }

  function selectCompetition(competition: Competition) {
    selectedCompetition.value = competition;
    selectedLeagueCode.value = competition.leagueCode || competition.league;
    selectedDivision.value = competition.division || "";
    fetchGames();
  }

  function clearSelection() {
    selectedLeague.value = null;
    selectedCompetition.value = null;
    selectedLeagueCode.value = "";
    selectedDivision.value = "";
    games.value = [];
    error.value = null;
  }

  function resetAll() {
    selectedSeason.value = "";
    selectedCategory.value = "";
    selectedLeagueCode.value = "";
    selectedDivision.value = "";
    selectedLeague.value = null;
    selectedCompetition.value = null;
    leagues.value = [];
    competitions.value = [];
    availableDivisions.value = [];
    games.value = [];
    error.value = null;
  }

  return {
    // State
    leagues,
    games,
    competitions,
    selectedLeague,
    selectedCompetition,
    selectedSeason,
    selectedCategory,
    selectedLeagueCode,
    selectedDivision,
    loading,
    error,

    // Static
    availableSeasons,
    availableCategories,

    // Computed
    filteredLeagues,
    availableLeagues,
    availableDivisions,
    goztepeGames,
    upcomingGames,
    recentGames,

    // Actions
    fetchLeagues,
    fetchCompetitions,
    fetchDivisions,
    fetchGames,
    changeSeason,
    changeCategory,
    changeLeague,
    changeDivision,
    selectCompetition,
    clearSelection,
    resetAll,
  };
});
