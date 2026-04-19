import type {
  CacheStatus,
  FixtureRequest,
  FixtureResponse,
  Game,
  LeagueDefinition,
  CompetitionRequest,
  CompetitionsResponse,
  StandingsRequest,
  StandingsResponse,
} from "@/types";

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...options?.headers,
    },
    ...options,
  });

  if (!response.ok) {
    const message = await response.text().catch(() => response.statusText);
    throw new Error(`API hatası (${response.status}): ${message}`);
  }

  return response.json() as Promise<T>;
}

export const fixtureService = {
  // Fixture API endpoints
  async getLeagues(): Promise<LeagueDefinition[]> {
    const response = await request<
      { code: string; displayName: string; category: string }[]
    >("/api/volleyball/fixture/leagues");
    return response.map((league) => ({
      code: league.code,
      name: league.displayName,
      categoryCode: league.category,
      displayName: league.displayName,
    }));
  },

  getGames(payload: FixtureRequest, forceRefresh = false): Promise<Game[]> {
    const url = forceRefresh
      ? "/api/volleyball/fixture/games?forceRefresh=true"
      : "/api/volleyball/fixture/games";
    return request<FixtureResponse>(url, {
      method: "POST",
      body: JSON.stringify(payload),
    }).then((response) => {
      // Extract games from the nested structure
      const allGames: Game[] = [];
      for (const leagueGroup of response.leagues) {
        allGames.push(...leagueGroup.games);
      }
      return allGames;
    });
  },

  getCacheStatus(): Promise<CacheStatus> {
    return request<CacheStatus>("/api/volleyball/fixture/cache/status");
  },

  clearCache(seasonId?: string): Promise<void> {
    const url = seasonId
      ? `/api/volleyball/fixture/cache?seasonId=${encodeURIComponent(seasonId)}`
      : "/api/volleyball/fixture/cache";
    return request<void>(url, { method: "DELETE" });
  },
};

export const standingsService = {
  // Standings API endpoints
  getCompetitions(payload: CompetitionRequest): Promise<CompetitionsResponse> {
    return request<CompetitionsResponse>(
      "/api/volleyball/standings/competitions",
      {
        method: "POST",
        body: JSON.stringify(payload),
      },
    );
  },

  getStandings(payload: StandingsRequest): Promise<StandingsResponse> {
    return request<StandingsResponse>("/api/volleyball/standings", {
      method: "POST",
      body: JSON.stringify(payload),
    });
  },

  getGoztepeCompetitions(
    seasonId: string,
    category?: string,
  ): Promise<{
    seasonId: string;
    category?: string;
    total: number;
    competitions: any[];
  }> {
    const url = category
      ? `/api/volleyball/standings/competitions/goztepe?seasonId=${seasonId}&category=${category}`
      : `/api/volleyball/standings/competitions/goztepe?seasonId=${seasonId}`;
    return request(url);
  },

  getCacheStatus(): Promise<CacheStatus> {
    return request<CacheStatus>("/api/volleyball/standings/cache/status");
  },

  clearCache(seasonId?: string): Promise<void> {
    const url = seasonId
      ? `/api/volleyball/standings/cache?seasonId=${encodeURIComponent(seasonId)}`
      : "/api/volleyball/standings/cache";
    return request<void>(url, { method: "DELETE" });
  },
};
