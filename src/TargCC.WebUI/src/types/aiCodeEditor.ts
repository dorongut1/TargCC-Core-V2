/**
 * TypeScript types for AI Code Editor
 * Matches the backend API models
 */

export interface CodeModificationRequest {
  originalCode: string;
  instruction: string;
  tableName: string;
  schema?: string;
  relatedTables?: string[];
  conversationId?: string;
  userPreferences?: Record<string, string>;
}

export interface CodeModificationResponse {
  success: boolean;
  modifiedCode: string;
  originalCode: string;
  changes: CodeChange[];
  validation: ValidationResult;
  errorMessage?: string;
  conversationId: string;
  explanation?: string;
}

export interface CodeChange {
  lineNumber: number;
  type: 'Addition' | 'Deletion' | 'Modification';
  description: string;
  oldValue: string;
  newValue: string;
}

export interface ValidationResult {
  isValid: boolean;
  errors: ValidationError[];
  warnings: ValidationWarning[];
  hasBreakingChanges: boolean;
}

export interface ValidationError {
  message: string;
  lineNumber?: number;
  severity: 'Info' | 'Warning' | 'Error' | 'Critical';
}

export interface ValidationWarning {
  message: string;
  lineNumber?: number;
}

export interface CodeValidationRequest {
  originalCode: string;
  modifiedCode: string;
}

export interface CodeDiffRequest {
  originalCode: string;
  modifiedCode: string;
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
}

export interface AICodeEditorState {
  originalCode: string;
  currentCode: string;
  conversationId: string;
  chatHistory: ChatMessage[];
  isModifying: boolean;
  validationErrors: ValidationError[];
  validationWarnings: ValidationWarning[];
  changes: CodeChange[];
}
