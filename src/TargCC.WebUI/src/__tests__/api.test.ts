/**
 * API Service Tests
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TargccApiService } from '../services/api';
import axios from 'axios';

// Mock axios
vi.mock('axios');
const mockedAxios = axios as any;

describe('TargccApiService', () => {
  let apiService: TargccApiService;

  beforeEach(() => {
    apiService = new TargccApiService('http://localhost:5000/api');
    mockedAxios.create.mockReturnValue({
      get: vi.fn(),
      post: vi.fn(),
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() },
      },
    });
  });

  it('creates an instance with base URL', () => {
    expect(apiService).toBeDefined();
  });

  it('has getTables method', () => {
    expect(typeof apiService.getTables).toBe('function');
  });

  it('has getTable method', () => {
    expect(typeof apiService.getTable).toBe('function');
  });

  it('has generateCode method', () => {
    expect(typeof apiService.generateCode).toBe('function');
  });

  it('has generateAll method', () => {
    expect(typeof apiService.generateAll).toBe('function');
  });

  it('has getSuggestions method', () => {
    expect(typeof apiService.getSuggestions).toBe('function');
  });

  it('has sendChatMessage method', () => {
    expect(typeof apiService.sendChatMessage).toBe('function');
  });

  it('has analyzeSecurity method', () => {
    expect(typeof apiService.analyzeSecurity).toBe('function');
  });

  it('has analyzeQuality method', () => {
    expect(typeof apiService.analyzeQuality).toBe('function');
  });

  it('has getDashboardStats method', () => {
    expect(typeof apiService.getDashboardStats).toBe('function');
  });
});
