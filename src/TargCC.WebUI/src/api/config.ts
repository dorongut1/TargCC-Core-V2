/**
 * API Configuration
 * Central configuration for API endpoints and settings
 */

export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  TIMEOUT: 90000, // 90 seconds - reasonable timeout for single operations
  DEFAULT_HEADERS: {
    'Content-Type': 'application/json',
  },
};

export const API_ENDPOINTS = {
  // Schema endpoints
  schemas: '/api/schema',
  schemaDetail: (name: string) => `/api/schema/${encodeURIComponent(name)}`,
  schemaRefresh: (name: string) => `/api/schema/${encodeURIComponent(name)}/refresh`,

  // Generation endpoints
  generate: '/api/generate',
  generationStatus: (id: string) => `/api/generate/${encodeURIComponent(id)}`,
  generationHistory: '/api/generate/history',

  // AI Code Editor endpoints
  aiCodeModify: '/api/ai/code/modify',
  aiCodeValidate: '/api/ai/code/validate',
  aiCodeDiff: '/api/ai/code/diff',

  // Health endpoint
  health: '/api/health',
};

/**
 * Create fetch options with timeout
 */
export function createFetchOptions(options: RequestInit = {}): RequestInit {
  return {
    ...options,
    headers: {
      ...API_CONFIG.DEFAULT_HEADERS,
      ...options.headers,
    },
    signal: AbortSignal.timeout(API_CONFIG.TIMEOUT),
  };
}
