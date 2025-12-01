import { useContext } from 'react';
import { ConnectionContext } from '../contexts/ConnectionContext';
import type { ConnectionContextType } from '../contexts/ConnectionContext';

/**
 * Hook to access connection context
 * @throws {Error} if used outside ConnectionProvider
 */
export function useConnection(): ConnectionContextType {
  const context = useContext(ConnectionContext);

  if (context === undefined) {
    throw new Error('useConnection must be used within a ConnectionProvider');
  }

  return context;
}
