import type { CacheStatus, FixtureRequest, Game, LeagueDefinition } from '@/types'

const BASE_URL = import.meta.env.VITE_API_BASE_URL

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    ...options,
  })

  if (!response.ok) {
    const message = await response.text().catch(() => response.statusText)
    throw new Error(`API hatası (${response.status}): ${message}`)
  }

  return response.json() as Promise<T>
}

export const volleyballService = {
  getLeagues(): Promise<LeagueDefinition[]> {
    return request<LeagueDefinition[]>('/api/volleyball/leagues')
  },

  getGames(payload: FixtureRequest, forceRefresh = false): Promise<Game[]> {
    const url = forceRefresh
      ? '/api/volleyball/games?forceRefresh=true'
      : '/api/volleyball/games'
    return request<Game[]>(url, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getCacheStatus(): Promise<CacheStatus> {
    return request<CacheStatus>('/api/volleyball/cache/status')
  },

  clearCache(seasonId?: string): Promise<void> {
    const url = seasonId
      ? `/api/volleyball/cache?seasonId=${encodeURIComponent(seasonId)}`
      : '/api/volleyball/cache'
    return request<void>(url, { method: 'DELETE' })
  },
}
