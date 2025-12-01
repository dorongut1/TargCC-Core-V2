import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { act } from 'react';
import { ConnectionProvider } from '../../contexts/ConnectionContext';
import { useConnection } from '../../hooks/useConnection';
import type { Connection } from '../../api/connectionApi';
import * as connectionApi from '../../api/connectionApi';

// Test component to access context
function TestComponent() {
  const { selectedConnection, connections, isLoading, error } = useConnection();

  return (
    <div>
      <div data-testid="loading">{isLoading ? 'loading' : 'loaded'}</div>
      <div data-testid="error">{error || 'no-error'}</div>
      <div data-testid="connections-count">{connections.length}</div>
      <div data-testid="selected">{selectedConnection?.name || 'none'}</div>
    </div>
  );
}

// Test component to test setSelectedConnection
function TestSetConnection() {
  const { connections, selectedConnection, setSelectedConnection } = useConnection();

  return (
    <div>
      <div data-testid="selected">{selectedConnection?.name || 'none'}</div>
      {connections.map((conn) => (
        <button
          key={conn.id}
          data-testid={`select-${conn.id}`}
          onClick={() => setSelectedConnection(conn)}
        >
          {conn.name}
        </button>
      ))}
      <button
        data-testid="clear"
        onClick={() => setSelectedConnection(null)}
      >
        Clear
      </button>
    </div>
  );
}

const mockConnections: Connection[] = [
  {
    id: '1',
    name: 'Test Connection 1',
    server: 'localhost',
    database: 'TestDB1',
    connectionString: 'Server=localhost;Database=TestDB1;',
    lastUsed: new Date('2025-12-01'),
    created: new Date('2025-11-01'),
    useIntegratedSecurity: true,
  },
  {
    id: '2',
    name: 'Test Connection 2',
    server: 'server2',
    database: 'TestDB2',
    connectionString: 'Server=server2;Database=TestDB2;',
    lastUsed: new Date('2025-12-01'),
    created: new Date('2025-11-01'),
    useIntegratedSecurity: false,
    username: 'testuser',
  },
];

describe('ConnectionContext', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should load connections on mount', async () => {
    const fetchConnectionsSpy = vi.spyOn(connectionApi, 'fetchConnections')
      .mockResolvedValueOnce(mockConnections);

    render(
      <ConnectionProvider>
        <TestComponent />
      </ConnectionProvider>
    );

    // Initially loading
    expect(screen.getByTestId('loading')).toHaveTextContent('loading');

    // Wait for connections to load
    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('loaded');
    });

    expect(screen.getByTestId('connections-count')).toHaveTextContent('2');
    expect(screen.getByTestId('error')).toHaveTextContent('no-error');
    expect(fetchConnectionsSpy).toHaveBeenCalledTimes(1);
  });

  it('should handle fetch error gracefully', async () => {
    const fetchConnectionsSpy = vi.spyOn(connectionApi, 'fetchConnections')
      .mockRejectedValueOnce(new Error('Network error'));

    render(
      <ConnectionProvider>
        <TestComponent />
      </ConnectionProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('loaded');
    });

    expect(screen.getByTestId('error')).toHaveTextContent('Network error');
    expect(screen.getByTestId('connections-count')).toHaveTextContent('0');
  });

  it('should restore selected connection from localStorage', async () => {
    localStorage.setItem('targcc_selected_connection', '1');

    vi.spyOn(connectionApi, 'fetchConnections')
      .mockResolvedValueOnce(mockConnections);

    render(
      <ConnectionProvider>
        <TestComponent />
      </ConnectionProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('loaded');
    });

    expect(screen.getByTestId('selected')).toHaveTextContent('Test Connection 1');
  });

  it('should clear localStorage if saved connection does not exist', async () => {
    localStorage.setItem('targcc_selected_connection', 'nonexistent-id');

    vi.spyOn(connectionApi, 'fetchConnections')
      .mockResolvedValueOnce(mockConnections);

    render(
      <ConnectionProvider>
        <TestComponent />
      </ConnectionProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toHaveTextContent('loaded');
    });

    expect(screen.getByTestId('selected')).toHaveTextContent('none');
    expect(localStorage.getItem('targcc_selected_connection')).toBeNull();
  });

  it('should set selected connection and persist to localStorage', async () => {
    vi.spyOn(connectionApi, 'fetchConnections')
      .mockResolvedValueOnce(mockConnections);

    const { getByTestId } = render(
      <ConnectionProvider>
        <TestSetConnection />
      </ConnectionProvider>
    );

    await waitFor(() => {
      expect(getByTestId('select-1')).toBeInTheDocument();
    });

    // Select connection
    act(() => {
      getByTestId('select-1').click();
    });

    await waitFor(() => {
      expect(getByTestId('selected')).toHaveTextContent('Test Connection 1');
    });

    expect(localStorage.getItem('targcc_selected_connection')).toBe('1');
  });

  it('should clear selected connection and remove from localStorage', async () => {
    localStorage.setItem('targcc_selected_connection', '1');

    vi.spyOn(connectionApi, 'fetchConnections')
      .mockResolvedValueOnce(mockConnections);

    const { getByTestId } = render(
      <ConnectionProvider>
        <TestSetConnection />
      </ConnectionProvider>
    );

    await waitFor(() => {
      expect(getByTestId('selected')).toHaveTextContent('Test Connection 1');
    });

    // Clear selection
    act(() => {
      getByTestId('clear').click();
    });

    await waitFor(() => {
      expect(getByTestId('selected')).toHaveTextContent('none');
    });

    expect(localStorage.getItem('targcc_selected_connection')).toBeNull();
  });

  it('should refresh connections', async () => {
    const fetchConnectionsSpy = vi.spyOn(connectionApi, 'fetchConnections')
      .mockResolvedValueOnce(mockConnections)
      .mockResolvedValueOnce([...mockConnections, {
        id: '3',
        name: 'Test Connection 3',
        server: 'server3',
        database: 'TestDB3',
        connectionString: 'Server=server3;Database=TestDB3;',
        lastUsed: new Date('2025-12-01'),
        created: new Date('2025-11-01'),
        useIntegratedSecurity: true,
      }]);

    function TestRefresh() {
      const { connections, refreshConnections } = useConnection();

      return (
        <div>
          <div data-testid="count">{connections.length}</div>
          <button data-testid="refresh" onClick={refreshConnections}>
            Refresh
          </button>
        </div>
      );
    }

    const { getByTestId } = render(
      <ConnectionProvider>
        <TestRefresh />
      </ConnectionProvider>
    );

    await waitFor(() => {
      expect(getByTestId('count')).toHaveTextContent('2');
    });

    // Refresh
    act(() => {
      getByTestId('refresh').click();
    });

    await waitFor(() => {
      expect(getByTestId('count')).toHaveTextContent('3');
    });

    expect(fetchConnectionsSpy).toHaveBeenCalledTimes(2);
  });

  it('should throw error when useConnection is used outside provider', () => {
    // Mock console.error to suppress error output in test
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    expect(() => {
      render(<TestComponent />);
    }).toThrow('useConnection must be used within a ConnectionProvider');

    consoleErrorSpy.mockRestore();
  });
});
