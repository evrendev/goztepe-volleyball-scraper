export interface Game {
  date: string;
  time: string;
  venue: string;
  homeTeam: string;
  awayTeam: string;
  score: string;
  division: string;
  group: string;
  league: string;
}

export interface LeagueDefinition {
  code: string;
  name: string;
  categoryCode: string;
  displayName: string;
}

export interface FixtureRequest {
  seasonId: string;
  leagues?: string[];
  organizationId?: string;
  gender?: string;
}

export interface CacheStatus {
  cachedKeys: string[];
}
