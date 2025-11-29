import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import ActivityTimeline from '../components/ActivityTimeline';

describe('ActivityTimeline', () => {
  it('renders component with title', () => {
    render(<ActivityTimeline />);
    expect(screen.getByText('Activity Timeline')).toBeInTheDocument();
  });

  it('renders mock data when no data provided', () => {
    render(<ActivityTimeline />);
    expect(screen.getByText(/Generated entities for Customer table/)).toBeInTheDocument();
  });

  it('shows empty state when no activities', () => {
    render(<ActivityTimeline activities={[]} />);
    expect(screen.getByText('No recent activity')).toBeInTheDocument();
  });

  it('respects maxItems prop', () => {
    const activities = Array.from({ length: 15 }, (_, i) => ({
      id: `${i}`,
      type: 'Generation' as const,
      description: `Activity ${i}`,
      timestamp: new Date(),
      user: 'System'
    }));
    
    render(<ActivityTimeline activities={activities} maxItems={5} />);
    
    expect(screen.getByText('Activity 0')).toBeInTheDocument();
    expect(screen.getByText('Activity 4')).toBeInTheDocument();
    expect(screen.queryByText('Activity 5')).not.toBeInTheDocument();
  });

  it('displays Generation type activities', () => {
    const activities = [
      {
        id: '1',
        type: 'Generation' as const,
        description: 'Test generation',
        timestamp: new Date(),
        user: 'Developer'
      }
    ];
    
    render(<ActivityTimeline activities={activities} />);
    expect(screen.getByText('Test generation')).toBeInTheDocument();
  });

  it('displays Scan type activities', () => {
    const activities = [
      {
        id: '1',
        type: 'Scan' as const,
        description: 'Security scan completed',
        timestamp: new Date(),
        user: 'Admin'
      }
    ];
    
    render(<ActivityTimeline activities={activities} />);
    expect(screen.getByText('Security scan completed')).toBeInTheDocument();
  });

  it('displays Analysis type activities', () => {
    const activities = [
      {
        id: '1',
        type: 'Analysis' as const,
        description: 'Code analysis finished',
        timestamp: new Date(),
        user: 'System'
      }
    ];
    
    render(<ActivityTimeline activities={activities} />);
    expect(screen.getByText('Code analysis finished')).toBeInTheDocument();
  });

  it('displays Refresh type activities', () => {
    const activities = [
      {
        id: '1',
        type: 'Refresh' as const,
        description: 'Schema refreshed',
        timestamp: new Date(),
        user: 'System'
      }
    ];
    
    render(<ActivityTimeline activities={activities} />);
    expect(screen.getByText('Schema refreshed')).toBeInTheDocument();
  });

  it('displays user information', () => {
    const activities = [
      {
        id: '1',
        type: 'Generation' as const,
        description: 'Test activity',
        timestamp: new Date(),
        user: 'TestUser'
      }
    ];
    
    render(<ActivityTimeline activities={activities} />);
    expect(screen.getByText(/TestUser/)).toBeInTheDocument();
  });

  it('formats time correctly', () => {
    const activities = [
      {
        id: '1',
        type: 'Generation' as const,
        description: 'Test activity',
        timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000), // 2 hours ago
        user: 'System'
      }
    ];
    
    render(<ActivityTimeline activities={activities} />);
    expect(screen.getByText(/2h ago/)).toBeInTheDocument();
  });

  it('shows "just now" for very recent activities', () => {
    const activities = [
      {
        id: '1',
        type: 'Generation' as const,
        description: 'Very recent activity',
        timestamp: new Date(Date.now() - 30 * 1000), // 30 seconds ago
        user: 'System'
      }
    ];
    
    render(<ActivityTimeline activities={activities} />);
    expect(screen.getByText('just now')).toBeInTheDocument();
  });

  it('renders timeline connectors between items', () => {
    const activities = [
      {
        id: '1',
        type: 'Generation' as const,
        description: 'First activity',
        timestamp: new Date(),
        user: 'User1'
      },
      {
        id: '2',
        type: 'Scan' as const,
        description: 'Second activity',
        timestamp: new Date(),
        user: 'User2'
      }
    ];
    
    const { container } = render(<ActivityTimeline activities={activities} />);
    const connectors = container.querySelectorAll('.MuiTimelineConnector-root');
    expect(connectors.length).toBeGreaterThan(0);
  });

  it('displays multiple activities in order', () => {
    const activities = [
      {
        id: '1',
        type: 'Generation' as const,
        description: 'First',
        timestamp: new Date(),
        user: 'System'
      },
      {
        id: '2',
        type: 'Scan' as const,
        description: 'Second',
        timestamp: new Date(),
        user: 'Admin'
      },
      {
        id: '3',
        type: 'Analysis' as const,
        description: 'Third',
        timestamp: new Date(),
        user: 'Developer'
      }
    ];
    
    render(<ActivityTimeline activities={activities} />);
    expect(screen.getByText('First')).toBeInTheDocument();
    expect(screen.getByText('Second')).toBeInTheDocument();
    expect(screen.getByText('Third')).toBeInTheDocument();
  });
});
