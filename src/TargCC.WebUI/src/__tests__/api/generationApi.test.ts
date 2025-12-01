/**
 * Tests for generationApi
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import {
  fetchGenerationHistory,
  fetchGenerationStatus,
  fetchLastGeneration,
  clearGenerationHistory,
  fetchMultipleGenerationStatuses,
} from '../../api/generationApi';
import type { GenerationHistory, GenerationStatus } from '../../api/generationApi';

// Mock fetch globally
const mockFetch = vi.fn();
global.fetch = mockFetch;

describe('generationApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('fetchGenerationHistory', () => {
    it('should fetch all history when no table name provided', async () => {
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

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockHistory,
      });

      const result = await fetchGenerationHistory();

      expect(result).toEqual(mockHistory);
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/generation/history'),
        expect.objectContaining({
          method: 'GET',
        })
      );
    });

    it('should fetch history for specific table', async () => {
      const mockHistory: GenerationHistory[] = [];

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockHistory,
      });

      await fetchGenerationHistory('Users');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/generation/history/Users'),
        expect.anything()
      );
    });

    it('should throw error on failed request', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        statusText: 'Server Error',
      });

      await expect(fetchGenerationHistory()).rejects.toThrow(
        'Failed to fetch generation history: Server Error'
      );
    });
  });

  describe('fetchGenerationStatus', () => {
    it('should fetch status for a table', async () => {
      const mockStatus: GenerationStatus = {
        tableName: 'Users',
        status: 'Generated',
        lastGenerated: '2025-12-01T10:00:00Z',
        success: true,
        filesGenerated: 2,
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockStatus,
      });

      const result = await fetchGenerationStatus('Users');

      expect(result).toEqual(mockStatus);
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/generation/status/Users'),
        expect.objectContaining({
          method: 'GET',
        })
      );
    });

    it('should handle URL encoding for table names', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({}),
      });

      await fetchGenerationStatus('Order Details');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('Order%20Details'),
        expect.anything()
      );
    });

    it('should throw error on failed request', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        statusText: 'Not Found',
      });

      await expect(fetchGenerationStatus('NonExistent')).rejects.toThrow(
        'Failed to fetch generation status: Not Found'
      );
    });
  });

  describe('fetchLastGeneration', () => {
    it('should return last generation when history exists', async () => {
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

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockHistory,
      });

      const result = await fetchLastGeneration('Users');

      expect(result).toEqual(mockHistory[0]);
    });

    it('should return null when no history exists', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => [],
      });

      const result = await fetchLastGeneration('Users');

      expect(result).toBeNull();
    });

    it('should return null on error', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'));

      const result = await fetchLastGeneration('Users');

      expect(result).toBeNull();
    });
  });

  describe('clearGenerationHistory', () => {
    it('should send DELETE request', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
      });

      await clearGenerationHistory();

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/generation/history'),
        expect.objectContaining({
          method: 'DELETE',
        })
      );
    });

    it('should throw error on failed request', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        statusText: 'Forbidden',
      });

      await expect(clearGenerationHistory()).rejects.toThrow(
        'Failed to clear generation history: Forbidden'
      );
    });
  });

  describe('fetchMultipleGenerationStatuses', () => {
    it('should fetch status for multiple tables', async () => {
      const mockStatus1: GenerationStatus = {
        tableName: 'Users',
        status: 'Generated',
        lastGenerated: '2025-12-01T10:00:00Z',
        success: true,
        filesGenerated: 2,
      };

      const mockStatus2: GenerationStatus = {
        tableName: 'Products',
        status: 'Not Generated',
        success: false,
        filesGenerated: 0,
      };

      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: async () => mockStatus1,
        })
        .mockResolvedValueOnce({
          ok: true,
          json: async () => mockStatus2,
        });

      const result = await fetchMultipleGenerationStatuses(['Users', 'Products']);

      expect(result.size).toBe(2);
      expect(result.get('Users')).toEqual(mockStatus1);
      expect(result.get('Products')).toEqual(mockStatus2);
    });

    it('should handle errors for individual tables', async () => {
      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: async () => ({
            tableName: 'Users',
            status: 'Generated',
            success: true,
            filesGenerated: 2,
          }),
        })
        .mockRejectedValueOnce(new Error('Network error'));

      const result = await fetchMultipleGenerationStatuses(['Users', 'Products']);

      expect(result.size).toBe(2);
      expect(result.get('Users')?.status).toBe('Generated');
      expect(result.get('Products')?.status).toBe('Not Generated'); // Default fallback
    });

    it('should return empty map for empty input', async () => {
      const result = await fetchMultipleGenerationStatuses([]);

      expect(result.size).toBe(0);
      expect(mockFetch).not.toHaveBeenCalled();
    });
  });
});
