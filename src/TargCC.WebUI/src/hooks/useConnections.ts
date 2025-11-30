import { useState, useEffect, useCallback } from 'react';
import {
  fetchConnections,
  addConnection,
  updateConnection,
  deleteConnection,
  testConnection,
  type Connection,
} from '../api/connectionApi';

export interface UseConnectionsResult {
  connections: Connection[];
  loading: boolean;
  error: string | null;
  selectedConnection: Connection | null;
  addNewConnection: (connection: Omit<Connection, 'id' | 'created' | 'lastUsed'>) => Promise<void>;
  updateExistingConnection: (connection: Connection) => Promise<void>;
  removeConnection: (id: string) => Promise<void>;
  testConnectionString: (connectionString: string) => Promise<boolean>;
  selectConnection: (connection: Connection | null) => void;
  refresh: () => Promise<void>;
}

/**
 * Hook for managing database connections
 */
export function useConnections(): UseConnectionsResult {
  const [connections, setConnections] = useState<Connection[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedConnection, setSelectedConnection] = useState<Connection | null>(null);

  // Load connections on mount
  const loadConnections = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await fetchConnections();
      setConnections(data);
      
      // Auto-select the most recently used connection
      if (data.length > 0 && !selectedConnection) {
        setSelectedConnection(data[0]);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load connections';
      setError(errorMessage);
      console.error('Error loading connections:', err);
    } finally {
      setLoading(false);
    }
  }, [selectedConnection]);

  useEffect(() => {
    loadConnections();
  }, []);

  // Add new connection
  const addNewConnection = useCallback(async (connection: Omit<Connection, 'id' | 'created' | 'lastUsed'>) => {
    setLoading(true);
    setError(null);
    try {
      const added = await addConnection(connection);
      setConnections(prev => [added, ...prev]);
      setSelectedConnection(added);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to add connection';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  // Update existing connection
  const updateExistingConnection = useCallback(async (connection: Connection) => {
    setLoading(true);
    setError(null);
    try {
      await updateConnection(connection);
      setConnections(prev => prev.map(c => (c.id === connection.id ? connection : c)));
      if (selectedConnection?.id === connection.id) {
        setSelectedConnection(connection);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update connection';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, [selectedConnection]);

  // Remove connection
  const removeConnection = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await deleteConnection(id);
      setConnections(prev => prev.filter(c => c.id !== id));
      if (selectedConnection?.id === id) {
        setSelectedConnection(null);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete connection';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, [selectedConnection]);

  // Test connection string
  const testConnectionString = useCallback(async (connectionString: string): Promise<boolean> => {
    setLoading(true);
    setError(null);
    try {
      return await testConnection(connectionString);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to test connection';
      setError(errorMessage);
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  // Select connection
  const selectConnection = useCallback((connection: Connection | null) => {
    setSelectedConnection(connection);
  }, []);

  // Refresh connections
  const refresh = useCallback(async () => {
    await loadConnections();
  }, [loadConnections]);

  return {
    connections,
    loading,
    error,
    selectedConnection,
    addNewConnection,
    updateExistingConnection,
    removeConnection,
    testConnectionString,
    selectConnection,
    refresh,
  };
}
