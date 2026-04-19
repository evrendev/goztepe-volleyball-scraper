import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { standingsService, fixtureService } from "@/services/volleyballService";
import type {
  Competition,
  CompetitionRequest,
  StandingsRequest,
  StandingsResponse,
  LeagueDefinition,
  StandingsRow,
  GameResult,
} from "@/types/volleyball";

export const useStandingsStore = defineStore("standings", () => {
  const competitions = ref<Competition[]>([]);
  const leagues = ref<LeagueDefinition[]>([]);
  const selectedCompetition = ref<Competition | null>(null);
  const standings = ref<StandingsResponse | null>(null);
  const goztepeCompetitions = ref<any[]>([]);

  const selectedSeason = ref("");
  const selectedCategory = ref("");
  const selectedLeagueCode = ref("");

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

  const goztepeStandings = computed(() => {
    if (!standings.value?.hasGoztepe) return null;
    return standings.value.standings.find((s: StandingsRow) => s.isGoztepe);
  });

  const goztepeGames = computed(() => {
    if (!standings.value?.hasGoztepe) return [];
    return standings.value.games.filter((g: GameResult) => g.isGoztepe);
  });

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
      const request: CompetitionRequest = {
        seasonId: selectedSeason.value,
        category: selectedCategory.value,
        leagueCode: selectedLeagueCode.value,
      };

      const response = await standingsService.getCompetitions(request);
      competitions.value = response.competitions;
    } catch (e) {
      error.value = e instanceof Error ? e.message : "Yarışmalar yüklenemedi";
      competitions.value = [];
    } finally {
      loading.value = false;
    }
  }

  async function fetchStandings() {
    if (!selectedCompetition.value) return;

    loading.value = true;
    error.value = null;
    try {
      const request: StandingsRequest = {
        seasonId: selectedSeason.value,
        category: selectedCategory.value,
        leagueCode: selectedLeagueCode.value,
        competitionName: selectedCompetition.value.name,
      };

      standings.value = await standingsService.getStandings(request);
    } catch (e) {
      error.value = e instanceof Error ? e.message : "Puan durumu yüklenemedi";
      standings.value = null;
    } finally {
      loading.value = false;
    }
  }

  async function fetchGoztepeCompetitions() {
    loading.value = true;
    error.value = null;
    try {
      const response = await standingsService.getGoztepeCompetitions(
        selectedSeason.value,
        selectedCategory.value,
      );
      goztepeCompetitions.value = response.competitions;
    } catch (e) {
      error.value =
        e instanceof Error ? e.message : "Göztepe yarışmaları yüklenemedi";
      goztepeCompetitions.value = [];
    } finally {
      loading.value = false;
    }
  }

  async function selectCompetition(competition: Competition) {
    selectedCompetition.value = competition;
    await fetchStandings();
  }

  async function changeCategory(category: string) {
    selectedCategory.value = category;
    selectedLeagueCode.value = "";
    competitions.value = [];
    selectedCompetition.value = null;
    standings.value = null;
    // Force reload leagues to ensure fresh data
    leagues.value = [];
    await fetchLeagues();
  }

  async function changeLeague(leagueCode: string) {
    selectedLeagueCode.value = leagueCode;
    competitions.value = [];
    selectedCompetition.value = null;
    standings.value = null;
    // Always fetch competitions when league changes (if all required fields are present)
    if (leagueCode && selectedSeason.value && selectedCategory.value) {
      await fetchCompetitions();
    }
  }

  async function changeSeason(season: string) {
    selectedSeason.value = season;
    selectedLeagueCode.value = "";
    competitions.value = [];
    selectedCompetition.value = null;
    standings.value = null;
    goztepeCompetitions.value = [];
    // Force reload leagues to ensure fresh data
    leagues.value = [];
    await fetchLeagues();
  }

  function resetAll() {
    selectedSeason.value = "";
    selectedCategory.value = "";
    selectedLeagueCode.value = "";
    leagues.value = [];
    competitions.value = [];
    selectedCompetition.value = null;
    standings.value = null;
    goztepeCompetitions.value = [];
    error.value = null;
  }

  return {
    // State
    competitions,
    leagues,
    selectedCompetition,
    standings,
    goztepeCompetitions,
    selectedSeason,
    selectedCategory,
    selectedLeagueCode,
    loading,
    error,

    // Computed
    availableSeasons,
    availableCategories,
    availableLeagues,
    goztepeStandings,
    goztepeGames,

    // Actions
    fetchLeagues,
    fetchCompetitions,
    fetchStandings,
    fetchGoztepeCompetitions,
    resetAll,
    selectCompetition,
    changeCategory,
    changeLeague,
    changeSeason,
  };
});
