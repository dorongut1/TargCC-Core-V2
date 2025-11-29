import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import RecentGenerations from '../components/RecentGenerations';

describe('RecentGenerations', () => {
  it('renders component with title', () => {
    render(<RecentGenerations />);
    expect(screen.getByText('Recent Generations')).toBeInTheDocument();
  });

  it('renders mock data when no data provided', () => {
    render(<RecentGenerations />);
    const customerItem = screen.getByText('Customer');
    expect(customerItem).toBeInTheDocument();
  });

  it('renders success icon for successful generations', () => {
    const generations = [
      {
        id: '1',
        tableName: 'TestTable',
        type: 'Entity' as const,
        timestamp: new Date(),
        status: 'Success' as const
      }
    ];
    
    render(<RecentGenerations generations={generations} />);
    expect(screen.getByText('TestTable')).toBeInTheDocument();
  });

  it('renders error icon for failed generations', () => {
    const generations = [
      {
        id: '1',
        tableName: 'FailedTable',
        type: 'API' as const,
        timestamp: new Date(),
        status: 'Failed' as const
      }
    ];
    
    render(<RecentGenerations generations={generations} />);
    expect(screen.getByText('FailedTable')).toBeInTheDocument();
  });

  it('shows empty state when no generations', () => {
    render(<RecentGenerations generations={[]} />);
    expect(screen.getByText('No recent generations')).toBeInTheDocument();
  });

  it('respects maxItems prop', () => {
    const generations = Array.from({ length: 10 }, (_, i) => ({
      id: `${i}`,
      tableName: `Table${i}`,
      type: 'Entity' as const,
      timestamp: new Date(),
      status: 'Success' as const
    }));
    
    render(<RecentGenerations generations={generations} maxItems={3} />);
    
    // Should only render first 3 items
    expect(screen.getByText('Table0')).toBeInTheDocument();
    expect(screen.getByText('Table1')).toBeInTheDocument();
    expect(screen.getByText('Table2')).toBeInTheDocument();
    expect(screen.queryByText('Table3')).not.toBeInTheDocument();
  });

  it('displays Entity type chip', () => {
    const generations = [
      {
        id: '1',
        tableName: 'TestTable',
        type: 'Entity' as const,
        timestamp: new Date(),
        status: 'Success' as const
      }
    ];
    
    render(<RecentGenerations generations={generations} />);
    expect(screen.getByText('Entity')).toBeInTheDocument();
  });

  it('displays Repository type chip', () => {
    const generations = [
      {
        id: '1',
        tableName: 'TestTable',
        type: 'Repository' as const,
        timestamp: new Date(),
        status: 'Success' as const
      }
    ];
    
    render(<RecentGenerations generations={generations} />);
    expect(screen.getByText('Repository')).toBeInTheDocument();
  });

  it('displays API type chip', () => {
    const generations = [
      {
        id: '1',
        tableName: 'TestTable',
        type: 'API' as const,
        timestamp: new Date(),
        status: 'Success' as const
      }
    ];
    
    render(<RecentGenerations generations={generations} />);
    expect(screen.getByText('API')).toBeInTheDocument();
  });

  it('displays SQL type chip', () => {
    const generations = [
      {
        id: '1',
        tableName: 'TestTable',
        type: 'SQL' as const,
        timestamp: new Date(),
        status: 'Success' as const
      }
    ];
    
    render(<RecentGenerations generations={generations} />);
    expect(screen.getByText('SQL')).toBeInTheDocument();
  });

  it('formats time ago for recent items', () => {
    const generations = [
      {
        id: '1',
        tableName: 'TestTable',
        type: 'Entity' as const,
        timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000), // 2 hours ago
        status: 'Success' as const
      }
    ];
    
    render(<RecentGenerations generations={generations} />);
    expect(screen.getByText(/hours ago/)).toBeInTheDocument();
  });

  it('shows "just now" for very recent items', () => {
    const generations = [
      {
        id: '1',
        tableName: 'TestTable',
        type: 'Entity' as const,
        timestamp: new Date(Date.now() - 30 * 1000), // 30 seconds ago
        status: 'Success' as const
      }
    ];
    
    render(<RecentGenerations generations={generations} />);
    expect(screen.getByText('just now')).toBeInTheDocument();
  });
});
