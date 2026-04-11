import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { standingsService } from "@/services/volleyballService";
import type {
  Competition,
  CompetitionRequest,
  StandingsRequest,
  StandingsResponse,
  LeagueDefinition,
} from "@/types";

export const useStandingsStore = defineStore("standings", () => {
  const competitions = ref<Competition[]>([]);
  const selectedCompetition = ref<Competition | null>(null);
  const standings = ref<StandingsResponse | null>(null);
  const goztepeCompetitions = ref<any[]>([]);

  const selectedSeason = ref("2025-2026");
  const selectedCategory = ref("GK"); // Genç Kızlar
  const selectedLeagueCode = ref("GKSL"); // Genç Kızlar Süper Ligi

  const loading = ref(false);
  const error = ref<string | null>(null);

  const availableSeasons = ["2025-2026", "2024-2025", "2023-2024", "2022-2023"];
  const availableCategories = [
    { code: "GK", name: "Genç Kızlar" },
    { code: "KK", name: "Küçük Kızlar" },
    { code: "BB", name: "Büyük Bayan" },
    { code: "YK", name: "Yıldız Kızlar" },
    { code: "MiK", name: "Minik Kızlar" },
  ];

  const availableLeagues = computed(() => {
    // Backend'den gelen ligler burada olacak, şimdilik sabit değerler
    return [
      { code: "GKSL", name: "Genç Kızlar Süper Ligi", category: "GK" },
      { code: "GK1L", name: "Genç Kızlar 1. Ligi", category: "GK" },
      { code: "KKSL", name: "Küçük Kızlar Süper Ligi", category: "KK" },
      { code: "BBSL", name: "Büyük Bayalar Süper Ligi", category: "BB" },
      { code: "YKSL", name: "Yıldız Kızlar Süper Ligi", category: "YK" },
    ].filter((l) => l.category === selectedCategory.value);
  });

  const goztepeStandings = computed(() => {
    if (!standings.value?.hasGoztepe) return null;
    return standings.value.standings.find((s) => s.isGoztepe);
  });

  const goztepeGames = computed(() => {
    if (!standings.value?.hasGoztepe) return [];
    return standings.value.games.filter(
      (g) => g.isGoztepeHome || g.isGoztepeAway,
    );
  });

  async function fetchCompetitions() {
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

      // İlk yarışmayı varsayılan olarak seç
      if (competitions.value.length > 0) {
        selectedCompetition.value = competitions.value[0];
      }
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
    // Kategori değiştiğinde lig de değişmeli
    const newLeagues = availableLeagues.value;
    if (newLeagues.length > 0) {
      selectedLeagueCode.value = newLeagues[0].code;
    }
    competitions.value = [];
    selectedCompetition.value = null;
    standings.value = null;
    await fetchCompetitions();
  }

  async function changeLeague(leagueCode: string) {
    selectedLeagueCode.value = leagueCode;
    competitions.value = [];
    selectedCompetition.value = null;
    standings.value = null;
    await fetchCompetitions();
  }

  async function changeSeason(season: string) {
    selectedSeason.value = season;
    competitions.value = [];
    selectedCompetition.value = null;
    standings.value = null;
    goztepeCompetitions.value = [];
    await fetchCompetitions();
  }

  return {
    // State
    competitions,
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
    fetchCompetitions,
    fetchStandings,
    fetchGoztepeCompetitions,
    selectCompetition,
    changeCategory,
    changeLeague,
    changeSeason,
  };
});
