// Fixture/Game Types
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
  category?: string;
  division?: string;
  group?: string;
  organisationId?: string;
  gender?: string;
}

export interface FixtureResponse {
  total: number;
  season: string;
  cachedLeagues: number;
  filters: {
    division?: string;
    category?: string;
    matchType?: string;
    group?: string;
    round?: string;
    week?: string;
  };
  leagues: Array<{
    league: string;
    count: number;
    games: Game[];
  }>;
}

// Standings Types
export interface Competition {
  name: string;
  displayName: string;
  league: string;
  leagueCode: string;
  category: string;
  division?: string;
  group?: string;
  hasGoztepe: boolean;
}

export interface CompetitionRequest {
  seasonId: string;
  category: string;
  leagueCode?: string;
}

export interface CompetitionsResponse {
  seasonId: string;
  category: string;
  leagueCode: string;
  total: number;
  competitions: Competition[];
}

export interface StandingsRow {
  position: number;
  teamName: string;
  played: number;
  won: number;
  lost: number;
  sets: string;
  points: string;
  isGoztepe: boolean;
}

export interface GameResult {
  rowNo: number;
  date: string;
  time: string;
  homeTeam: string;
  awayTeam: string;
  score: string;
  result: string;
  isGoztepeHome: boolean;
  isGoztepeAway: boolean;
}

export interface StandingsRequest {
  seasonId: string;
  category: string;
  leagueCode: string;
  competitionName: string;
}

export interface StandingsResponse {
  competitionName: string;
  seasonId: string;
  hasGoztepe: boolean;
  standings: StandingsRow[];
  games: GameResult[];
}

// Cache Types
export interface CacheStatus {
  totalCachedKeys: number;
  keys: string[];
}
