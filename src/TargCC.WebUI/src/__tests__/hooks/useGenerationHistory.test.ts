/**
 * Tests for useGenerationHistory hook
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useGenerationHistory, useTableGenerationStatus } from '../../hooks/useGenerationHistory';
import * as generationApi from '../../api/generationApi';
import type { GenerationHistory, GenerationStatus } from '../../api/generationApi';

// Mock the API module
vi.mock('../../api/generationApi', () => ({
  fetchGenerationHistory: vi.fn(),
  fetchGenerationStatus: vi.fn(),
  fetchLastGeneration: vi.fn(),
  clearGenerationHistory: vi.fn(),
}));

describe('useGenerationHistory', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should load history on mount', async () => {
    const mockHistory: GenerationHistory[] = [
      {
        id: '1',
        tableName: 'Users',
        schemaName: 'dbo',
        generatedAt: '2025-12-01T10:00:00Z',
        filesGenerated: ['User.cs'],
        success: true,
        errors: [],
        warnings: [],
        options: {
          generateEntity: true,
          generateRepository: false,
          generateService: false,
          generateController: false,
          generateTests: false,
          overwriteExisting: false,
        },
      },
    ];

    vi.mocked(generationApi.fetchGenerationHistory).mockResolvedValue(mockHistory);

    const { result } = renderHook(() => useGenerationHistory());

    // Initially loading
    expect(result.current.loading).toBe(true);
    expect(result.current.history).toEqual([]);

    // Wait for data to load
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.history).toEqual(mockHistory);
    expect(result.current.error).toBeNull();
    expect(generationApi.fetchGenerationHistory).toHaveBeenCalledWith(undefined);
  });

  it('should filter history by table name', async () => {
    const mockHistory: GenerationHistory[] = [
      {
        id: '1',
        tableName: 'Users',
        schemaName: 'dbo',
        generatedAt: '2025-12-01T10:00:00Z',
        filesGenerated: ['User.cs'],
        success: true,
        errors: [],
        warnings: [],
        options: {
          generateEntity: true,
          generateRepository: false,
          generateService: false,
          generateController: false,
          generateTests: false,
          overwriteExisting: false,
        },
      },
    ];

    vi.mocked(generationApi.fetchGenerationHistory).mockResolvedValue(mockHistory);

    const { result } = renderHook(() => useGenerationHistory({ tableName: 'Users' }));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(generationApi.fetchGenerationHistory).toHaveBeenCalledWith('Users');
    expect(result.current.history).toEqual(mockHistory);
  });

  it('should handle API errors gracefully', async () => {
    const mockError = new Error('API Error');
    vi.mocked(generationApi.fetchGenerationHistory).mockRejectedValue(mockError);

    const { result } = renderHook(() => useGenerationHistory());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.error).toEqual(mockError);
    expect(result.current.history).toEqual([]);
  });

  it('should refresh history manually', async () => {
    const mockHistory: GenerationHistory[] = [];
    vi.mocked(generationApi.fetchGenerationHistory).mockResolvedValue(mockHistory);

    const { result } = renderHook(() => useGenerationHistory());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    // Clear previous calls
    vi.clearAllMocks();

    // Trigger refresh
    await result.current.refresh();

    expect(generationApi.fetchGenerationHistory).toHaveBeenCalledTimes(1);
  });

  it('should clear history', async () => {
    vi.mocked(generationApi.fetchGenerationHistory).mockResolvedValue([]);
    vi.mocked(generationApi.clearGenerationHistory).mockResolvedValue();

    const { result } = renderHook(() => useGenerationHistory());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    await result.current.clear();

    expect(generationApi.clearGenerationHistory).toHaveBeenCalledTimes(1);
  });

  it('should get status for a specific table', async () => {
    const mockStatus: GenerationStatus = {
      tableName: 'Users',
      status: 'Generated',
      lastGenerated: '2025-12-01T10:00:00Z',
      success: true,
      filesGenerated: 2,
    };

    vi.mocked(generationApi.fetchGenerationHistory).mockResolvedValue([]);
    vi.mocked(generationApi.fetchGenerationStatus).mockResolvedValue(mockStatus);

    const { result } = renderHook(() => useGenerationHistory());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    const status = await result.current.getStatus('Users');

    expect(status).toEqual(mockStatus);
    expect(generationApi.fetchGenerationStatus).toHaveBeenCalledWith('Users');
  });

  it('should handle getStatus errors gracefully', async () => {
    vi.mocked(generationApi.fetchGenerationHistory).mockResolvedValue([]);
    vi.mocked(generationApi.fetchGenerationStatus).mockRejectedValue(new Error('API Error'));

    const { result } = renderHook(() => useGenerationHistory());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    const status = await result.current.getStatus('Users');

    expect(status).toBeNull();
  });

  it('should get last generation for a specific table', async () => {
    const mockGeneration: GenerationHistory = {
      id: '1',
      tableName: 'Users',
      schemaName: 'dbo',
      generatedAt: '2025-12-01T10:00:00Z',
      filesGenerated: ['User.cs'],
      success: true,
      errors: [],
      warnings: [],
      options: {
        generateEntity: true,
        generateRepository: false,
        generateService: false,
        generateController: false,
        generateTests: false,
        overwriteExisting: false,
      },
    };

    vi.mocked(generationApi.fetchGenerationHistory).mockResolvedValue([]);
    vi.mocked(generationApi.fetchLastGeneration).mockResolvedValue(mockGeneration);

    const { result } = renderHook(() => useGenerationHistory());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    const lastGen = await result.current.getLastGeneration('Users');

    expect(lastGen).toEqual(mockGeneration);
    expect(generationApi.fetchLastGeneration).toHaveBeenCalledWith('Users');
  });
});

describe('useTableGenerationStatus', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should load status for a specific table', async () => {
    const mockStatus: GenerationStatus = {
      tableName: 'Users',
      status: 'Generated',
      lastGenerated: '2025-12-01T10:00:00Z',
      success: true,
      filesGenerated: 2,
    };

    vi.mocked(generationApi.fetchGenerationStatus).mockResolvedValue(mockStatus);

    const { result } = renderHook(() => useTableGenerationStatus('Users'));

    expect(result.current.loading).toBe(true);

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.status).toEqual(mockStatus);
    expect(result.current.error).toBeNull();
  });

  it('should handle API errors', async () => {
    const mockError = new Error('API Error');
    vi.mocked(generationApi.fetchGenerationStatus).mockRejectedValue(mockError);

    const { result } = renderHook(() => useTableGenerationStatus('Users'));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.error).toEqual(mockError);
    expect(result.current.status).toBeNull();
  });

  it('should refresh status manually', async () => {
    const mockStatus: GenerationStatus = {
      tableName: 'Users',
      status: 'Generated',
      lastGenerated: '2025-12-01T10:00:00Z',
      success: true,
      filesGenerated: 2,
    };

    vi.mocked(generationApi.fetchGenerationStatus).mockResolvedValue(mockStatus);

    const { result } = renderHook(() => useTableGenerationStatus('Users'));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    vi.clearAllMocks();

    await result.current.refresh();

    expect(generationApi.fetchGenerationStatus).toHaveBeenCalledTimes(1);
  });
});
