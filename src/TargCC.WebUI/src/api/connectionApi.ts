import { API_CONFIG } from './config';

export interface Connection {
  id: string;
  name: string;
  server: string;
  database: string;
  connectionString: string;
  lastUsed: Date;
  created: Date;
  useIntegratedSecurity: boolean;
  username?: string;
}

export interface TestConnectionRequest {
  connectionString: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

/**
 * Fetch all saved connections
 */
export async function fetchConnections(): Promise<Connection[]> {
  try {
    const response = await fetch(`${API_CONFIG.BASE_URL}/api/connections`, {
      headers: API_CONFIG.DEFAULT_HEADERS,
    });

    const result: ApiResponse<Connection[]> = await response.json();
    if (result.success && result.data) {
      return result.data;
    }
    throw new Error(result.error || 'Failed to fetch connections');
  } catch (error) {
    console.error('Error fetching connections:', error);
    throw error;
  }
}

/**
 * Add a new connection
 */
export async function addConnection(connection: Omit<Connection, 'id' | 'created' | 'lastUsed'>): Promise<Connection> {
  try {
    const response = await fetch(`${API_CONFIG.BASE_URL}/api/connections`, {
      method: 'POST',
      headers: API_CONFIG.DEFAULT_HEADERS,
      body: JSON.stringify(connection),
    });

    const result: ApiResponse<Connection> = await response.json();
    if (result.success && result.data) {
      return result.data;
    }
    throw new Error(result.error || 'Failed to add connection');
  } catch (error) {
    console.error('Error adding connection:', error);
    throw error;
  }
}

/**
 * Update an existing connection
 */
export async function updateConnection(connection: Connection): Promise<void> {
  try {
    const response = await fetch(`${API_CONFIG.BASE_URL}/api/connections/${connection.id}`, {
      method: 'PUT',
      headers: API_CONFIG.DEFAULT_HEADERS,
      body: JSON.stringify(connection),
    });

    const result: ApiResponse<void> = await response.json();
    if (!result.success) {
      throw new Error(result.error || 'Failed to update connection');
    }
  } catch (error) {
    console.error('Error updating connection:', error);
    throw error;
  }
}

/**
 * Delete a connection
 */
export async function deleteConnection(id: string): Promise<void> {
  try {
    const response = await fetch(`${API_CONFIG.BASE_URL}/api/connections/${id}`, {
      method: 'DELETE',
      headers: API_CONFIG.DEFAULT_HEADERS,
    });

    const result: ApiResponse<void> = await response.json();
    if (!result.success) {
      throw new Error(result.error || 'Failed to delete connection');
    }
  } catch (error) {
    console.error('Error deleting connection:', error);
    throw error;
  }
}

/**
 * Test a connection string
 */
export async function testConnection(connectionString: string): Promise<boolean> {
  try {
    const response = await fetch(`${API_CONFIG.BASE_URL}/api/connections/test`, {
      method: 'POST',
      headers: API_CONFIG.DEFAULT_HEADERS,
      body: JSON.stringify({ connectionString }),
    });

    const result: any = await response.json();
    return result.IsValid ?? result.isValid ?? false;
  } catch (error) {
    console.error('Error testing connection:', error);
    return false;
  }
}

/**
 * Fetch a single connection by ID
 */
export async function fetchConnection(id: string): Promise<Connection> {
  try {
    const response = await fetch(`${API_CONFIG.BASE_URL}/api/connections/${id}`, {
      headers: API_CONFIG.DEFAULT_HEADERS,
    });

    const result: ApiResponse<Connection> = await response.json();
    if (result.success && result.data) {
      return result.data;
    }
    throw new Error(result.error || 'Failed to fetch connection');
  } catch (error) {
    console.error('Error fetching connection:', error);
    throw error;
  }
}
