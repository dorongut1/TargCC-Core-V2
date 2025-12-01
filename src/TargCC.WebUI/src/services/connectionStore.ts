/**
 * Connection Store
 * Singleton store for managing the active database connection
 */

export class ConnectionStore {
  private static instance: ConnectionStore;
  private currentConnectionString: string | null = null;

  private constructor() {}

  static getInstance(): ConnectionStore {
    if (!ConnectionStore.instance) {
      ConnectionStore.instance = new ConnectionStore();
    }
    return ConnectionStore.instance;
  }

  setConnectionString(connectionString: string | null): void {
    this.currentConnectionString = connectionString;
  }

  getConnectionString(): string | null {
    return this.currentConnectionString;
  }

  hasConnection(): boolean {
    return this.currentConnectionString !== null && this.currentConnectionString !== '';
  }
}

export const connectionStore = ConnectionStore.getInstance();
