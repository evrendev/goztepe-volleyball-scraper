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
  rank: number;
  teamName: string;
  logoUrl: string;
  played: number;
  won: number;
  lost: number;
  setsWon: number;
  setsLost: number;
  points: number;
  setAverage: string;
  pointsWon: number;
  pointsLost: number;
  pointAverage: number;
  w30: number;
  w31: number;
  w32: number;
  l23: number;
  l13: number;
  l03: number;
  isGoztepe: boolean;
}

export interface GameResult {
  rowNo: number;
  date: string;
  time: string;
  venue: string;
  homeTeam: string;
  awayTeam: string;
  homeScore: number;
  awayScore: number;
  setResults: string;
  isPlayed: boolean;
  isGoztepe: boolean;
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
