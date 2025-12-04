/**
 * API client for AI Code Editor operations
 */

import { API_CONFIG } from './config';
import type {
  CodeModificationRequest,
  CodeModificationResponse,
  CodeValidationRequest,
  CodeDiffRequest,
  ValidationResult,
  CodeChange,
} from '../types/aiCodeEditor';

/**
 * Modifies code using AI based on natural language instructions
 */
export async function modifyCode(request: CodeModificationRequest): Promise<CodeModificationResponse> {
  const url = `${API_CONFIG.BASE_URL}/api/ai/code/modify`;

  const response = await fetch(url, {
    method: 'POST',
    headers: API_CONFIG.DEFAULT_HEADERS,
    body: JSON.stringify(request),
    signal: AbortSignal.timeout(API_CONFIG.TIMEOUT),
  });

  if (!response.ok) {
    throw new Error(`Failed to modify code: ${response.statusText}`);
  }

  return await response.json();
}

/**
 * Validates modified code against original code
 */
export async function validateCode(request: CodeValidationRequest): Promise<{ success: boolean; validation: ValidationResult }> {
  const url = `${API_CONFIG.BASE_URL}/api/ai/code/validate`;

  const response = await fetch(url, {
    method: 'POST',
    headers: API_CONFIG.DEFAULT_HEADERS,
    body: JSON.stringify(request),
    signal: AbortSignal.timeout(API_CONFIG.TIMEOUT),
  });

  if (!response.ok) {
    throw new Error(`Failed to validate code: ${response.statusText}`);
  }

  return await response.json();
}

/**
 * Generates a diff between original and modified code
 */
export async function generateDiff(request: CodeDiffRequest): Promise<{ success: boolean; changes: CodeChange[] }> {
  const url = `${API_CONFIG.BASE_URL}/api/ai/code/diff`;

  const response = await fetch(url, {
    method: 'POST',
    headers: API_CONFIG.DEFAULT_HEADERS,
    body: JSON.stringify(request),
    signal: AbortSignal.timeout(API_CONFIG.TIMEOUT),
  });

  if (!response.ok) {
    throw new Error(`Failed to generate diff: ${response.statusText}`);
  }

  return await response.json();
}
