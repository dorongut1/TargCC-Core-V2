/**
 * TargCC WebUI Type Definitions
 * Core data models for the application
 */

/**
 * Represents a database table in the schema
 */
export interface Table {
  name: string;
  schema: string;
  columns: Column[];
  primaryKey?: string;
  foreignKeys: ForeignKey[];
  isGenerated: boolean;
  generationStatus: string;
  rowCount?: number;
  lastGenerated?: Date;
}

/**
 * Represents a table column
 */
export interface Column {
  name: string;
  dataType: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  maxLength?: number;
  defaultValue?: string;
  description?: string;
}

/**
 * Represents a foreign key relationship
 */
export interface ForeignKey {
  columnName: string;
  referencedTable: string;
  referencedColumn: string;
}

/**
 * Generation request options
 */
export interface GenerationRequest {
  tableNames: string[];
  connectionString: string;
  options: GenerationOptions;
}

/**
 * Generation options for code generation
 */
export interface GenerationOptions {
  generateEntity: boolean;
  generateRepository: boolean;
  generateService: boolean;
  generateController: boolean;
  generateTests: boolean;
  overwriteExisting: boolean;
}

/**
 * Generation result
 */
export interface GenerationResult {
  success: boolean;
  filesGenerated: string[];
  errors: string[];
  warnings: string[];
}

/**
 * AI suggestion from the suggestion engine
 */
export interface AISuggestion {
  type: 'naming' | 'relationship' | 'index' | 'validation' | 'security';
  severity: 'info' | 'warning' | 'error';
  tableName: string;
  columnName?: string;
  message: string;
  suggestion: string;
  autoFixAvailable: boolean;
}

/**
 * Security vulnerability finding
 */
export interface SecurityIssue {
  severity: 'low' | 'medium' | 'high' | 'critical';
  category: string;
  tableName: string;
  columnName?: string;
  description: string;
  recommendation: string;
  cweId?: string;
}

/**
 * Code quality analysis result
 */
export interface QualityReport {
  overallScore: number;
  grade: 'A' | 'B' | 'C' | 'D' | 'F';
  namingIssues: QualityIssue[];
  bestPracticeIssues: QualityIssue[];
  relationshipIssues: QualityIssue[];
}

/**
 * Individual quality issue
 */
export interface QualityIssue {
  severity: 'info' | 'warning' | 'error';
  location: string;
  issue: string;
  recommendation: string;
}

/**
 * Dashboard statistics
 */
export interface DashboardStats {
  totalTables: number;
  generatedTables: number;
  totalTests: number;
  testCoverage: number;
  lastGenerationTime?: Date;
  recentActivity: ActivityItem[];
}

/**
 * Recent activity item
 */
export interface ActivityItem {
  timestamp: Date;
  action: string;
  description: string;
  status: 'success' | 'warning' | 'error';
}

/**
 * Chat message for AI interaction
 */
export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
}

/**
 * API error response
 */
export interface ApiError {
  message: string;
  code?: string;
  details?: string[];
}
