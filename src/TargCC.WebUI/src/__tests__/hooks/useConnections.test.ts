import { describe, it, expect, beforeEach, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useConnections } from '../../hooks/useConnections';
import * as connectionApi from '../../api/connectionApi';

// Mock the API
vi.mock('../../api/connectionApi');

describe('useConnections', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('should initialize with empty connections', () => {
    vi.mocked(connectionApi.fetchConnections).mockResolvedValue([]);
    
    const { result } = renderHook(() => useConnections());
    
    expect(result.current.connections).toEqual([]);
    expect(result.current.selectedConnection).toBeNull();
    expect(result.current.loading).toBe(true);
  });

  it('should load connections on mount', async () => {
    const mockConnections = [
      {
        id: '1',
        name: 'Test Connection',
        server: 'localhost',
        database: 'TestDB',
        connectionString: 'Server=localhost;Database=TestDB;',
        useIntegratedSecurity: true,
        lastUsed: new Date().toISOString(),
        created: new Date().toISOString(),
      },
    ];
    
    vi.mocked(connectionApi.fetchConnections).mockResolvedValue(mockConnections);
    
    const { result } = renderHook(() => useConnections());
    
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
    
    expect(result.current.connections).toEqual(mockConnections);
    expect(result.current.selectedConnection).toEqual(mockConnections[0]);
  });

  it('should add new connection', async () => {
    const newConnection = {
      name: 'New Connection',
      server: 'server1',
      database: 'db1',
      connectionString: 'cs1',
      useIntegratedSecurity: true,
    };
    
    const addedConnection = { ...newConnection, id: '2', lastUsed: new Date().toISOString(), created: new Date().toISOString() };
    
    vi.mocked(connectionApi.fetchConnections).mockResolvedValue([]);
    vi.mocked(connectionApi.addConnection).mockResolvedValue(addedConnection);
    
    const { result } = renderHook(() => useConnections());
    
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
    
    await result.current.addConnection(newConnection);
    
    expect(connectionApi.addConnection).toHaveBeenCalledWith(newConnection);
    expect(result.current.connections).toContainEqual(addedConnection);
  });

  it('should delete connection', async () => {
    const mockConnections = [
      { id: '1', name: 'Connection 1', server: 'server1', database: 'db1', connectionString: 'cs1', useIntegratedSecurity: true, lastUsed: new Date().toISOString(), created: new Date().toISOString() },
      { id: '2', name: 'Connection 2', server: 'server2', database: 'db2', connectionString: 'cs2', useIntegratedSecurity: true, lastUsed: new Date().toISOString(), created: new Date().toISOString() },
    ];
    
    vi.mocked(connectionApi.fetchConnections).mockResolvedValue(mockConnections);
    vi.mocked(connectionApi.deleteConnection).mockResolvedValue();
    
    const { result } = renderHook(() => useConnections());
    
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
    
    await result.current.deleteConnection('1');
    
    expect(connectionApi.deleteConnection).toHaveBeenCalledWith('1');
    expect(result.current.connections).toHaveLength(1);
    expect(result.current.connections[0].id).toBe('2');
  });

  it('should test connection successfully', async () => {
    vi.mocked(connectionApi.fetchConnections).mockResolvedValue([]);
    vi.mocked(connectionApi.testConnection).mockResolvedValue(true);
    
    const { result } = renderHook(() => useConnections());
    
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
    
    const isValid = await result.current.testConnection('test-connection-string');
    
    expect(isValid).toBe(true);
    expect(connectionApi.testConnection).toHaveBeenCalledWith('test-connection-string');
  });

  it('should select connection', async () => {
    const mockConnections = [
      { id: '1', name: 'Connection 1', server: 'server1', database: 'db1', connectionString: 'cs1', useIntegratedSecurity: true, lastUsed: new Date().toISOString(), created: new Date().toISOString() },
      { id: '2', name: 'Connection 2', server: 'server2', database: 'db2', connectionString: 'cs2', useIntegratedSecurity: true, lastUsed: new Date().toISOString(), created: new Date().toISOString() },
    ];
    
    vi.mocked(connectionApi.fetchConnections).mockResolvedValue(mockConnections);
    
    const { result } = renderHook(() => useConnections());
    
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
    
    result.current.selectConnection('2');
    
    expect(result.current.selectedConnection?.id).toBe('2');
  });
});
