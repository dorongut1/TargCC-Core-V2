/**
 * TargCC API Service
 * HTTP client for communicating with TargCC backend
 */

import axios from 'axios';
import type { AxiosInstance, AxiosError } from 'axios';
import type {
  Table,
  GenerationRequest,
  GenerationResult,
  AISuggestion,
  SecurityIssue,
  QualityReport,
  DashboardStats,
  ChatMessage,
  ApiError,
} from '../types/models';

/**
 * API service class for all backend communication
 */
export class TargccApiService {
  private client: AxiosInstance;

  constructor(baseURL: string = 'http://localhost:5000/api') {
    this.client = axios.create({
      baseURL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor for logging
    this.client.interceptors.request.use(
      (config) => {
        console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`);
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor for error handling
    this.client.interceptors.response.use(
      (response) => response,
      (error: AxiosError<ApiError>) => {
        const apiError: ApiError = {
          message: error.response?.data?.message || error.message,
          code: error.code,
          details: error.response?.data?.details,
        };
        return Promise.reject(apiError);
      }
    );
  }

  // ========================================
  // Schema & Tables
  // ========================================

  /**
   * Get all tables from the database schema
   */
  async getTables(): Promise<Table[]> {
    const response = await this.client.get<Table[]>('/tables');
    return response.data;
  }

  /**
   * Get a specific table by name
   */
  async getTable(tableName: string): Promise<Table> {
    const response = await this.client.get<Table>(`/tables/${tableName}`);
    return response.data;
  }

  // ========================================
  // Code Generation
  // ========================================

  /**
   * Generate code for a specific table
   */
  async generateCode(request: GenerationRequest): Promise<GenerationResult> {
    const response = await this.client.post<GenerationResult>('/generate', request);
    return response.data;
  }

  /**
   * Generate code for all tables
   */
  async generateAll(): Promise<GenerationResult> {
    const response = await this.client.post<GenerationResult>('/generate/all');
    return response.data;
  }

  // ========================================
  // AI Services
  // ========================================

  /**
   * Get AI suggestions for schema improvements
   */
  async getSuggestions(tableName?: string): Promise<AISuggestion[]> {
    const url = tableName ? `/ai/suggestions?table=${tableName}` : '/ai/suggestions';
    const response = await this.client.get<AISuggestion[]>(url);
    return response.data;
  }

  /**
   * Send a chat message to the AI assistant
   */
  async sendChatMessage(message: string, conversationId?: string): Promise<ChatMessage> {
    const response = await this.client.post<ChatMessage>('/ai/chat', {
      message,
      conversationId,
    });
    return response.data;
  }

  /**
   * Analyze security vulnerabilities
   */
  async analyzeSecurity(): Promise<SecurityIssue[]> {
    const response = await this.client.get<SecurityIssue[]>('/ai/security');
    return response.data;
  }

  /**
   * Analyze code quality
   */
  async analyzeQuality(): Promise<QualityReport> {
    const response = await this.client.get<QualityReport>('/ai/quality');
    return response.data;
  }

  // ========================================
  // Dashboard
  // ========================================

  /**
   * Get dashboard statistics
   */
  async getDashboardStats(): Promise<DashboardStats> {
    const response = await this.client.get<DashboardStats>('/dashboard/stats');
    return response.data;
  }
}

// Export a singleton instance
export const apiService = new TargccApiService();
