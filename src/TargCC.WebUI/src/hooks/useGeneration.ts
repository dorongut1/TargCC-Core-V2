/**
 * useGeneration Hook
 * React hook for managing code generation process
 * Note: Currently not used - GenerationWizard component handles generation directly
 * Kept for potential future use with alternative UI flows
 */

import { useState, useCallback } from 'react';
import { generate } from '../api/generationApi';

/**
 * Generation status
 */
export type GenerationStatus = 'idle' | 'preparing' | 'generating' | 'completed' | 'error';

/**
 * Generation result
 */
export interface GenerationResult {
  id: string;
  status: GenerationStatus;
  progress: number;
  message: string;
  filesGenerated?: number;
  errors?: string[];
}

/**
 * Hook result interface
 */
export interface UseGenerationResult {
  isGenerating: boolean;
  status: GenerationStatus;
  progress: number;
  result: GenerationResult | null;
  error: string | null;
  startGeneration: (tables: string[]) => Promise<void>;
  reset: () => void;
}

/**
 * Hook for managing code generation
 */
export function useGeneration(): UseGenerationResult {
  const [isGenerating, setIsGenerating] = useState(false);
  const [status, setStatus] = useState<GenerationStatus>('idle');
  const [progress, setProgress] = useState(0);
  const [result, setResult] = useState<GenerationResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  /**
   * Start generation process
   */
  const startGeneration = useCallback(async (tables: string[]) => {
    setIsGenerating(true);
    setStatus('preparing');
    setProgress(0);
    setError(null);

    try {
      setStatus('generating');
      setProgress(25);

      // Call the actual generation API
      const result = await generate({
        tableNames: tables,
        options: {
          generateEntity: true,
          generateRepository: true,
          generateStoredProcedures: true,
          generateController: true,
        }
      });

      setProgress(75);

      if (result.success) {
        setStatus('completed');
        setProgress(100);
        setResult({
          id: `gen-${Date.now()}`,
          status: 'completed',
          progress: 100,
          message: result.message || 'Generation completed successfully',
          filesGenerated: tables.length * 4, // Entity, Repository, Controller, SQL per table
        });
      } else {
        throw new Error(result.message || 'Generation failed');
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Generation failed';
      setError(errorMessage);
      setStatus('error');
      console.error('Generation error:', err);
    } finally {
      setIsGenerating(false);
    }
  }, []);

  /**
   * Reset generation state
   */
  const reset = useCallback(() => {
    setIsGenerating(false);
    setStatus('idle');
    setProgress(0);
    setResult(null);
    setError(null);
  }, []);

  return {
    isGenerating,
    status,
    progress,
    result,
    error,
    startGeneration,
    reset,
  };
}
