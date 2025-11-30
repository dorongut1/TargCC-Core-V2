/**
 * useSchema Hook
 * React hook for loading and managing schema data
 */

import { useState, useEffect, useCallback } from 'react';
import { fetchSchemaDetails, refreshSchema } from '../api/schemaApi';
import type { DatabaseSchema } from '../types/schema';

/**
 * Hook result interface
 */
export interface UseSchemaResult {
  schema: DatabaseSchema | null;
  loading: boolean;
  error: string | null;
  refresh: () => Promise<void>;
  lastUpdated: Date | null;
  isConnected: boolean;
}

/**
 * Hook for loading schema data from API
 * @param schemaName - Name of the schema to load (null = no load)
 * @param autoLoad - Whether to automatically load on mount (default: true)
 */
export function useSchema(
  schemaName: string | null,
  autoLoad: boolean = true
): UseSchemaResult {
  const [schema, setSchema] = useState<DatabaseSchema | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  /**
   * Load schema from API
   */
  const loadSchema = useCallback(async (name: string) => {
    setLoading(true);
    setError(null);

    try {
      const data = await fetchSchemaDetails(name);
      setSchema(data);
      setLastUpdated(new Date());
      setIsConnected(true);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load schema';
      setError(errorMessage);
      setIsConnected(false);
      console.error('Schema load error:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Refresh schema (force reload from database)
   */
  const refresh = useCallback(async () => {
    if (!schemaName) {
      console.warn('Cannot refresh: no schema name provided');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const data = await refreshSchema(schemaName);
      setSchema(data);
      setLastUpdated(new Date());
      setIsConnected(true);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to refresh schema';
      setError(errorMessage);
      setIsConnected(false);
      console.error('Schema refresh error:', err);
    } finally {
      setLoading(false);
    }
  }, [schemaName]);

  /**
   * Auto-load effect
   */
  useEffect(() => {
    if (schemaName && autoLoad) {
      loadSchema(schemaName);
    }
  }, [schemaName, autoLoad, loadSchema]);

  return {
    schema,
    loading,
    error,
    refresh,
    lastUpdated,
    isConnected,
  };
}
