/**
 * Connection Store
 * Singleton store for managing the active database connection
 * Persists connection string in localStorage for persistence across page refreshes
 */

const STORAGE_KEY = 'targcc_connection_string';

export class ConnectionStore {
  private static instance: ConnectionStore;
  private currentConnectionString: string | null = null;

  private constructor() {
    // Load from localStorage on initialization
    this.loadFromStorage();
  }

  static getInstance(): ConnectionStore {
    if (!ConnectionStore.instance) {
      ConnectionStore.instance = new ConnectionStore();
    }
    return ConnectionStore.instance;
  }

  private loadFromStorage(): void {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        this.currentConnectionString = stored;
      }
    } catch (error) {
      console.error('Failed to load connection string from storage:', error);
    }
  }

  private saveToStorage(): void {
    try {
      if (this.currentConnectionString) {
        localStorage.setItem(STORAGE_KEY, this.currentConnectionString);
      } else {
        localStorage.removeItem(STORAGE_KEY);
      }
    } catch (error) {
      console.error('Failed to save connection string to storage:', error);
    }
  }

  setConnectionString(connectionString: string | null): void {
    this.currentConnectionString = connectionString;
    this.saveToStorage();
  }

  getConnectionString(): string | null {
    return this.currentConnectionString;
  }

  hasConnection(): boolean {
    return this.currentConnectionString !== null && this.currentConnectionString !== '';
  }

  clearConnection(): void {
    this.currentConnectionString = null;
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch (error) {
      console.error('Failed to clear connection string from storage:', error);
    }
  }
}

export const connectionStore = ConnectionStore.getInstance();
