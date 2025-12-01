/**
 * Hook for managing generation history data
 */

import { useState, useEffect, useCallback } from 'react';
import {
  fetchGenerationHistory,
  fetchGenerationStatus,
  fetchLastGeneration,
  clearGenerationHistory,
  type GenerationHistory,
  type GenerationStatus,
} from '../api/generationApi';

export interface UseGenerationHistoryOptions {
  tableName?: string;
  autoRefresh?: boolean;
  refreshInterval?: number; // in milliseconds
}

export interface UseGenerationHistoryResult {
  history: GenerationHistory[];
  loading: boolean;
  error: Error | null;
  refresh: () => Promise<void>;
  clear: () => Promise<void>;
  getStatus: (tableName: string) => Promise<GenerationStatus | null>;
  getLastGeneration: (tableName: string) => Promise<GenerationHistory | null>;
}

/**
 * Custom hook for managing generation history
 *
 * @param options - Configuration options
 * @returns Generation history state and operations
 *
 * @example
 * ```tsx
 * const { history, loading, refresh } = useGenerationHistory();
 *
 * // Filter by table
 * const { history } = useGenerationHistory({ tableName: 'Users' });
 *
 * // Auto-refresh every 30 seconds
 * const { history } = useGenerationHistory({
 *   autoRefresh: true,
 *   refreshInterval: 30000
 * });
 * ```
 */
export function useGenerationHistory(
  options: UseGenerationHistoryOptions = {}
): UseGenerationHistoryResult {
  const { tableName, autoRefresh = false, refreshInterval = 30000 } = options;

  const [history, setHistory] = useState<GenerationHistory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  /**
   * Fetches generation history from the API
   */
  const loadHistory = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await fetchGenerationHistory(tableName);
      setHistory(data);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to load generation history');
      setError(error);
      console.error('Error loading generation history:', error);
    } finally {
      setLoading(false);
    }
  }, [tableName]);

  /**
   * Manually refresh the history
   */
  const refresh = useCallback(async () => {
    await loadHistory();
  }, [loadHistory]);

  /**
   * Clear all generation history
   */
  const clear = useCallback(async () => {
    try {
      await clearGenerationHistory();
      await loadHistory(); // Reload after clearing
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to clear generation history');
      setError(error);
      console.error('Error clearing generation history:', error);
      throw error;
    }
  }, [loadHistory]);

  /**
   * Get generation status for a specific table
   */
  const getStatus = useCallback(async (tableName: string): Promise<GenerationStatus | null> => {
    try {
      return await fetchGenerationStatus(tableName);
    } catch (err) {
      console.error(`Error fetching status for ${tableName}:`, err);
      return null;
    }
  }, []);

  /**
   * Get last generation for a specific table
   */
  const getLastGeneration = useCallback(
    async (tableName: string): Promise<GenerationHistory | null> => {
      try {
        return await fetchLastGeneration(tableName);
      } catch (err) {
        console.error(`Error fetching last generation for ${tableName}:`, err);
        return null;
      }
    },
    []
  );

  // Load history on mount and when tableName changes
  useEffect(() => {
    loadHistory();
  }, [loadHistory]);

  // Auto-refresh if enabled
  useEffect(() => {
    if (!autoRefresh) return;

    const intervalId = setInterval(() => {
      loadHistory();
    }, refreshInterval);

    return () => clearInterval(intervalId);
  }, [autoRefresh, refreshInterval, loadHistory]);

  return {
    history,
    loading,
    error,
    refresh,
    clear,
    getStatus,
    getLastGeneration,
  };
}

/**
 * Hook for tracking generation status of a specific table
 *
 * @param tableName - The name of the table
 * @returns Generation status and operations
 *
 * @example
 * ```tsx
 * const { status, loading, refresh } = useTableGenerationStatus('Users');
 *
 * if (status?.status === 'Generated') {
 *   // Show "Generated" badge
 * }
 * ```
 */
export function useTableGenerationStatus(tableName: string) {
  const [status, setStatus] = useState<GenerationStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const loadStatus = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await fetchGenerationStatus(tableName);
      setStatus(data);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to load generation status');
      setError(error);
      console.error('Error loading generation status:', error);
    } finally {
      setLoading(false);
    }
  }, [tableName]);

  const refresh = useCallback(async () => {
    await loadStatus();
  }, [loadStatus]);

  useEffect(() => {
    loadStatus();
  }, [loadStatus]);

  return {
    status,
    loading,
    error,
    refresh,
  };
}
