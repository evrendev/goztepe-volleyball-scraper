import type {
  CacheStatus,
  FixtureRequest,
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
  getLeagues(): Promise<LeagueDefinition[]> {
    return request<LeagueDefinition[]>("/api/fixture/leagues");
  },

  getGames(payload: FixtureRequest, forceRefresh = false): Promise<Game[]> {
    const url = forceRefresh
      ? "/api/fixture/games?forceRefresh=true"
      : "/api/fixture/games";
    return request<Game[]>(url, {
      method: "POST",
      body: JSON.stringify(payload),
    });
  },

  getCacheStatus(): Promise<CacheStatus> {
    return request<CacheStatus>("/api/fixture/cache/status");
  },

  clearCache(seasonId?: string): Promise<void> {
    const url = seasonId
      ? `/api/fixture/cache?seasonId=${encodeURIComponent(seasonId)}`
      : "/api/fixture/cache";
    return request<void>(url, { method: "DELETE" });
  },
};

export const standingsService = {
  // Standings API endpoints
  getCompetitions(payload: CompetitionRequest): Promise<CompetitionsResponse> {
    return request<CompetitionsResponse>("/api/standings/competitions", {
      method: "POST",
      body: JSON.stringify(payload),
    });
  },

  getStandings(payload: StandingsRequest): Promise<StandingsResponse> {
    return request<StandingsResponse>("/api/standings", {
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
      ? `/api/standings/competitions/goztepe?seasonId=${seasonId}&category=${category}`
      : `/api/standings/competitions/goztepe?seasonId=${seasonId}`;
    return request(url);
  },

  getCacheStatus(): Promise<CacheStatus> {
    return request<CacheStatus>("/api/standings/cache/status");
  },

  clearCache(seasonId?: string): Promise<void> {
    const url = seasonId
      ? `/api/standings/cache?seasonId=${encodeURIComponent(seasonId)}`
      : "/api/standings/cache";
    return request<void>(url, { method: "DELETE" });
  },
};
