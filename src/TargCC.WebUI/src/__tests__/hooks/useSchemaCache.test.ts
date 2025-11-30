import { describe, it, expect, beforeEach, vi } from 'vitest';
import { schemaCache } from '../../hooks/useSchemaCache';

describe('schemaCache', () => {
  beforeEach(() => {
    schemaCache.clear();
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should cache data', () => {
    const key = 'test-key';
    const data = { test: 'data' };
    
    schemaCache.set(key, data);
    
    const cached = schemaCache.get(key);
    expect(cached).toEqual(data);
  });

  it('should return null for non-existent key', () => {
    const cached = schemaCache.get('non-existent');
    expect(cached).toBeNull();
  });

  it('should expire data after TTL', () => {
    const key = 'test-key';
    const data = { test: 'data' };
    
    schemaCache.set(key, data);
    
    // Fast-forward time by 6 minutes (TTL is 5 minutes)
    vi.advanceTimersByTime(6 * 60 * 1000);
    
    const cached = schemaCache.get(key);
    expect(cached).toBeNull();
  });

  it('should not expire data before TTL', () => {
    const key = 'test-key';
    const data = { test: 'data' };
    
    schemaCache.set(key, data);
    
    // Fast-forward time by 4 minutes (TTL is 5 minutes)
    vi.advanceTimersByTime(4 * 60 * 1000);
    
    const cached = schemaCache.get(key);
    expect(cached).toEqual(data);
  });

  it('should clear all cache', () => {
    schemaCache.set('key1', { data: 1 });
    schemaCache.set('key2', { data: 2 });
    
    schemaCache.clear();
    
    expect(schemaCache.get('key1')).toBeNull();
    expect(schemaCache.get('key2')).toBeNull();
  });

  it('should delete specific key', () => {
    schemaCache.set('key1', { data: 1 });
    schemaCache.set('key2', { data: 2 });
    
    schemaCache.invalidate('key1');
    
    expect(schemaCache.get('key1')).toBeNull();
    expect(schemaCache.get('key2')).toEqual({ data: 2 });
  });
});
