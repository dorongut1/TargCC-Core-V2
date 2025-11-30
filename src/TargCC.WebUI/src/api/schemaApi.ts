/**
 * Schema API Client
 * Handles all schema-related API calls
 */

import { API_CONFIG, API_ENDPOINTS, createFetchOptions } from './config';
import type { DatabaseSchema } from '../types/schema';

/**
 * Schema list item from API
 */
export interface SchemaListItem {
  name: string;
  displayName: string;
  tableCount: number;
  lastUpdated?: string;
}

/**
 * Table preview data from API
 */
export interface TablePreview {
  tableName: string;
  columns: string[];
  data: Array<Record<string, any>>;
  totalRowCount: number;
}

/**
 * Generic API response wrapper
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

/**
 * Handle API response and extract data
 */
async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`API Error (${response.status}): ${errorText || response.statusText}`);
  }

  const result: ApiResponse<T> = await response.json();
  
  if (!result.success) {
    throw new Error(result.error || result.message || 'Unknown API error');
  }

  if (!result.data) {
    throw new Error('API returned success but no data');
  }

  return result.data;
}

/**
 * Fetch list of available schemas
 */
export async function fetchSchemas(): Promise<SchemaListItem[]> {
  try {
    const response = await fetch(
      `${API_CONFIG.baseUrl}${API_ENDPOINTS.schemas}`,
      createFetchOptions({ method: 'GET' })
    );

    return await handleResponse<SchemaListItem[]>(response);
  } catch (error) {
    if (error instanceof Error) {
      throw new Error(`Failed to fetch schemas: ${error.message}`);
    }
    throw error;
  }
}

/**
 * Fetch detailed schema information
 */
export async function fetchSchemaDetails(schemaName: string): Promise<DatabaseSchema> {
  try {
    const response = await fetch(
      `${API_CONFIG.baseUrl}${API_ENDPOINTS.schemaDetail(schemaName)}`,
      createFetchOptions({ method: 'GET' })
    );

    return await handleResponse<DatabaseSchema>(response);
  } catch (error) {
    if (error instanceof Error) {
      throw new Error(`Failed to fetch schema '${schemaName}': ${error.message}`);
    }
    throw error;
  }
}

/**
 * Refresh schema from database (force reload)
 */
export async function refreshSchema(schemaName: string): Promise<DatabaseSchema> {
  try {
    const response = await fetch(
      `${API_CONFIG.baseUrl}${API_ENDPOINTS.schemaRefresh(schemaName)}`,
      createFetchOptions({ method: 'POST' })
    );

    return await handleResponse<DatabaseSchema>(response);
  } catch (error) {
    if (error instanceof Error) {
      throw new Error(`Failed to refresh schema '${schemaName}': ${error.message}`);
    }
    throw error;
  }
}

/**
 * Get table preview with sample data
 */
export async function fetchTablePreview(
  schemaName: string,
  tableName: string,
  rowCount: number = 10
): Promise<TablePreview> {
  try {
    const response = await fetch(
      `${API_CONFIG.baseUrl}/api/schema/${encodeURIComponent(schemaName)}/${encodeURIComponent(tableName)}/preview?rowCount=${rowCount}`,
      createFetchOptions({ method: 'GET' })
    );

    return await handleResponse<TablePreview>(response);
  } catch (error) {
    if (error instanceof Error) {
      throw new Error(`Failed to fetch table preview for '${tableName}': ${error.message}`);
    }
    throw error;
  }
}

/**
 * Check API health
 */
export async function checkHealth(): Promise<{ status: string; timestamp: string }> {
  try {
    const response = await fetch(
      `${API_CONFIG.baseUrl}${API_ENDPOINTS.health}`,
      createFetchOptions({ method: 'GET' })
    );

    if (!response.ok) {
      throw new Error('Health check failed');
    }

    return await response.json();
  } catch (error) {
    throw new Error('API is not accessible');
  }
}
