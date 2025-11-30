import { useCallback } from 'react';
import type { DatabaseSchema } from '../types/schema';

interface CacheEntry<T> {
  data: T;
  timestamp: number;
  expiresAt: number;
}

const CACHE_TTL = 5 * 60 * 1000; // 5 minutes

/**
 * Simple schema cache implementation
 */
class SchemaCache {
  private cache: Map<string, CacheEntry<any>> = new Map();

  set<T>(key: string, data: T, ttl: number = CACHE_TTL): void {
    this.cache.set(key, {
      data,
      timestamp: Date.now(),
      expiresAt: Date.now() + ttl,
    });
  }

  get<T>(key: string): T | null {
    const entry = this.cache.get(key);
    if (!entry) return null;

    if (Date.now() > entry.expiresAt) {
      this.cache.delete(key);
      return null;
    }

    return entry.data as T;
  }

  invalidate(key: string): void {
    this.cache.delete(key);
  }

  clear(): void {
    this.cache.clear();
  }

  has(key: string): boolean {
    const entry = this.cache.get(key);
    if (!entry) return false;
    if (Date.now() > entry.expiresAt) {
      this.cache.delete(key);
      return false;
    }
    return true;
  }

  getAge(key: string): number | null {
    const entry = this.cache.get(key);
    if (!entry) return null;
    return Date.now() - entry.timestamp;
  }
}

// Singleton instance
export const schemaCache = new SchemaCache();

/**
 * Hook for using schema cache
 */
export function useSchemaCache() {
  const setCache = useCallback(<T,>(key: string, data: T, ttl?: number) => {
    schemaCache.set(key, data, ttl);
  }, []);

  const getCache = useCallback(<T,>(key: string): T | null => {
    return schemaCache.get<T>(key);
  }, []);

  const invalidateCache = useCallback((key: string) => {
    schemaCache.invalidate(key);
  }, []);

  const clearCache = useCallback(() => {
    schemaCache.clear();
  }, []);

  const hasCache = useCallback((key: string): boolean => {
    return schemaCache.has(key);
  }, []);

  const getCacheAge = useCallback((key: string): number | null => {
    return schemaCache.getAge(key);
  }, []);

  return {
    setCache,
    getCache,
    invalidateCache,
    clearCache,
    hasCache,
    getCacheAge,
  };
}
