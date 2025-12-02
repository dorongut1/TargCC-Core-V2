/**
 * API client for generation history operations
 */

import { API_CONFIG } from './config';

export interface GenerationHistory {
  id: string;
  tableName: string;
  schemaName: string;
  generatedAt: string;
  filesGenerated: string[];
  success: boolean;
  errors: string[];
  warnings: string[];
  options: GenerationOptions;
}

export interface GenerationOptions {
  generateEntity: boolean;
  generateRepository: boolean;
  generateService: boolean;
  generateController: boolean;
  generateTests: boolean;
  generateStoredProcedures?: boolean;
  generateReactUI?: boolean;
  overwriteExisting: boolean;
}

export interface GenerationStatus {
  tableName: string;
  status: 'Not Generated' | 'Generated' | 'Modified' | 'Error';
  lastGenerated?: string;
  success: boolean;
  filesGenerated: number;
}

/**
 * Fetches all generation history, optionally filtered by table name
 */
export async function fetchGenerationHistory(tableName?: string): Promise<GenerationHistory[]> {
  const url = tableName
    ? `${API_CONFIG.BASE_URL}/api/generation/history/${encodeURIComponent(tableName)}`
    : `${API_CONFIG.BASE_URL}/api/generation/history`;

  const response = await fetch(url, {
    method: 'GET',
    headers: API_CONFIG.DEFAULT_HEADERS,
    signal: AbortSignal.timeout(API_CONFIG.TIMEOUT),
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch generation history: ${response.statusText}`);
  }

  return await response.json();
}

/**
 * Fetches the generation status for a specific table
 */
export async function fetchGenerationStatus(tableName: string): Promise<GenerationStatus> {
  const url = `${API_CONFIG.BASE_URL}/api/generation/status/${encodeURIComponent(tableName)}`;

  const response = await fetch(url, {
    method: 'GET',
    headers: API_CONFIG.DEFAULT_HEADERS,
    signal: AbortSignal.timeout(API_CONFIG.TIMEOUT),
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch generation status: ${response.statusText}`);
  }

  return await response.json();
}

/**
 * Fetches the details of the last generation for a specific table
 */
export async function fetchLastGeneration(tableName: string): Promise<GenerationHistory | null> {
  try {
    const history = await fetchGenerationHistory(tableName);
    return history.length > 0 ? history[0] : null;
  } catch (error) {
    console.error('Error fetching last generation:', error);
    return null;
  }
}

/**
 * Clears all generation history
 */
export async function clearGenerationHistory(): Promise<void> {
  const url = `${API_CONFIG.BASE_URL}/api/generation/history`;

  const response = await fetch(url, {
    method: 'DELETE',
    headers: API_CONFIG.DEFAULT_HEADERS,
    signal: AbortSignal.timeout(API_CONFIG.TIMEOUT),
  });

  if (!response.ok) {
    throw new Error(`Failed to clear generation history: ${response.statusText}`);
  }
}

/**
 * Fetches generation status for multiple tables at once
 */
export async function fetchMultipleGenerationStatuses(
  tableNames: string[]
): Promise<Map<string, GenerationStatus>> {
  const results = new Map<string, GenerationStatus>();

  // Fetch all statuses in parallel
  const promises = tableNames.map(async (tableName) => {
    try {
      const status = await fetchGenerationStatus(tableName);
      results.set(tableName, status);
    } catch (error) {
      console.error(`Error fetching status for ${tableName}:`, error);
      // Set default status on error
      results.set(tableName, {
        tableName,
        status: 'Not Generated',
        success: false,
        filesGenerated: 0,
      });
    }
  });

  await Promise.all(promises);
  return results;
}

/**
 * Request interface for code generation
 */
export interface GenerateRequest {
  tableNames: string[];
  options: GenerationOptions;
  projectPath?: string;
  connectionString?: string;
}

/**
 * Response interface for code generation
 */
export interface GenerateResponse {
  success: boolean;
  message?: string;
  generatedFiles?: string[];
  errors?: string[];
  executionTimeMs?: number;
}

/**
 * Triggers code generation for the specified tables with given options
 */
export async function generate(request: GenerateRequest): Promise<GenerateResponse> {
  const url = `${API_CONFIG.BASE_URL}/api/generate`;

  // Map UI options to backend request format - now with full granular control
  const backendRequest = {
    tableNames: request.tableNames,
    projectPath: request.projectPath,
    connectionString: request.connectionString,
    force: request.options.overwriteExisting,
    generateEntity: request.options.generateEntity,
    generateRepository: request.options.generateRepository,
    generateService: request.options.generateService,
    generateController: request.options.generateController,
    generateTests: request.options.generateTests,
    includeStoredProcedures: request.options.generateStoredProcedures ?? true,
  };

  const response = await fetch(url, {
    method: 'POST',
    headers: API_CONFIG.DEFAULT_HEADERS,
    signal: AbortSignal.timeout(API_CONFIG.TIMEOUT),
    body: JSON.stringify(backendRequest),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Generation failed: ${response.statusText}. ${errorText}`);
  }

  return await response.json();
}
