import React, { createContext, useState, useEffect, useCallback } from 'react';
import type { Connection } from '../api/connectionApi';
import { fetchConnections } from '../api/connectionApi';
import { connectionStore } from '../services/connectionStore';

const SELECTED_CONNECTION_KEY = 'targcc_selected_connection';

export interface ConnectionContextType {
  selectedConnection: Connection | null;
  setSelectedConnection: (connection: Connection | null) => void;
  connections: Connection[];
  isLoading: boolean;
  error: string | null;
  refreshConnections: () => Promise<void>;
}

export const ConnectionContext = createContext<ConnectionContextType | undefined>(undefined);

export interface ConnectionProviderProps {
  children: React.ReactNode;
}

export function ConnectionProvider({ children }: ConnectionProviderProps) {
  const [selectedConnection, setSelectedConnectionState] = useState<Connection | null>(null);
  const [connections, setConnections] = useState<Connection[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Load connections from API
  const refreshConnections = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await fetchConnections();
      setConnections(data);

      // Restore selected connection from localStorage if it exists
      const savedConnectionId = localStorage.getItem(SELECTED_CONNECTION_KEY);
      if (savedConnectionId) {
        const savedConnection = data.find(c => c.id === savedConnectionId);
        if (savedConnection) {
          setSelectedConnectionState(savedConnection);
          connectionStore.setConnectionString(savedConnection.connectionString);
        } else {
          // Connection no longer exists, clear from localStorage
          localStorage.removeItem(SELECTED_CONNECTION_KEY);
          connectionStore.setConnectionString(null);
        }
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load connections';
      setError(errorMessage);
      console.error('Error loading connections:', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Load connections on mount
  useEffect(() => {
    refreshConnections();
  }, [refreshConnections]);

  // Set selected connection and persist to localStorage and connection store
  const setSelectedConnection = useCallback((connection: Connection | null) => {
    setSelectedConnectionState(connection);
    if (connection) {
      localStorage.setItem(SELECTED_CONNECTION_KEY, connection.id);
      connectionStore.setConnectionString(connection.connectionString);
    } else {
      localStorage.removeItem(SELECTED_CONNECTION_KEY);
      connectionStore.setConnectionString(null);
    }
  }, []);

  const value: ConnectionContextType = {
    selectedConnection,
    setSelectedConnection,
    connections,
    isLoading,
    error,
    refreshConnections,
  };

  return (
    <ConnectionContext.Provider value={value}>
      {children}
    </ConnectionContext.Provider>
  );
}
