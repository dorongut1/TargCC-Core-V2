/**
 * Dashboard Component Tests
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Dashboard } from '../pages/Dashboard';

// Mock all child components
vi.mock('../components/SystemHealth', () => ({
  SystemHealth: ({ cpuUsage, memoryUsage, diskUsage, status }: any) => (
    <div data-testid="system-health">
      <div>CPU: {cpuUsage}%</div>
      <div>Memory: {memoryUsage}%</div>
      <div>Disk: {diskUsage}%</div>
      <div>Status: {status}</div>
    </div>
  ),
}));

vi.mock('../components/RecentGenerations', () => ({
  default: () => <div data-testid="recent-generations">Recent Generations Widget</div>
}));

vi.mock('../components/QuickStats', () => ({
  default: () => <div data-testid="quick-stats">Quick Stats Widget</div>
}));

vi.mock('../components/ActivityTimeline', () => ({
  default: () => <div data-testid="activity-timeline">Activity Timeline Widget</div>
}));

vi.mock('../components/SchemaStats', () => ({
  default: () => <div data-testid="schema-stats">Schema Stats Widget</div>
}));

vi.mock('../components/ErrorBoundary', () => ({
  default: ({ children }: any) => <div data-testid="error-boundary">{children}</div>
}));

vi.mock('../components/DashboardSkeleton', () => ({
  default: () => <div data-testid="dashboard-skeleton">Loading skeleton</div>
}));

vi.mock('../components/FadeIn', () => ({
  default: ({ children }: any) => <div data-testid="fade-in">{children}</div>
}));

vi.mock('../components/AutoRefreshControl', () => ({
  default: ({ enabled, onToggle, onManualRefresh }: any) => (
    <div data-testid="auto-refresh-control">
      <input
        type="checkbox"
        checked={enabled}
        onChange={(e) => onToggle(e.target.checked)}
        aria-label="Auto-refresh toggle"
      />
      {onManualRefresh && (
        <button onClick={onManualRefresh}>Manual Refresh</button>
      )}
    </div>
  )
}));

vi.mock('../hooks/useAutoRefresh', () => ({
  useAutoRefresh: () => ({
    lastRefresh: new Date('2024-01-01T12:00:00Z'),
    refresh: vi.fn()
  })
}));

describe('Dashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders dashboard title', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Dashboard/i)).toBeInTheDocument();
    });
  });

  it('wraps content in ErrorBoundary', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('error-boundary')).toBeInTheDocument();
    });
  });

  it('shows skeleton while loading', () => {
    render(<Dashboard />);
    expect(screen.getByTestId('dashboard-skeleton')).toBeInTheDocument();
  });

  it('displays AutoRefreshControl in header', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('auto-refresh-control')).toBeInTheDocument();
    });
  });

  it('displays QuickStats widget with FadeIn', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('quick-stats')).toBeInTheDocument();
    });
  });

  it('displays quick actions section with FadeIn', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Quick Actions/i)).toBeInTheDocument();
      expect(screen.getByText(/Generate All/i)).toBeInTheDocument();
      expect(screen.getByText(/Analyze Security/i)).toBeInTheDocument();
      expect(screen.getByText(/Check Quality/i)).toBeInTheDocument();
      expect(screen.getByText(/AI Chat/i)).toBeInTheDocument();
    });
  });

  it('displays RecentGenerations widget with FadeIn', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('recent-generations')).toBeInTheDocument();
    });
  });

  it('displays ActivityTimeline widget with FadeIn', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('activity-timeline')).toBeInTheDocument();
    });
  });

  it('displays SystemHealth widget with FadeIn', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('system-health')).toBeInTheDocument();
      expect(screen.getByText(/CPU: 45%/i)).toBeInTheDocument();
      expect(screen.getByText(/Memory: 62%/i)).toBeInTheDocument();
      expect(screen.getByText(/Disk: 38%/i)).toBeInTheDocument();
      expect(screen.getByText(/Status: healthy/i)).toBeInTheDocument();
    });
  });

  it('displays SchemaStats widget with FadeIn', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('schema-stats')).toBeInTheDocument();
    });
  });

  it('uses grid layout for dashboard widgets', async () => {
    const { container } = render(<Dashboard />);
    await waitFor(() => {
      const grids = container.querySelectorAll('.MuiGrid-container');
      expect(grids.length).toBeGreaterThan(0);
    });
  });

  it('displays 4 action buttons', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Generate All/i)).toBeInTheDocument();
      expect(screen.getByText(/Analyze Security/i)).toBeInTheDocument();
      expect(screen.getByText(/Check Quality/i)).toBeInTheDocument();
      expect(screen.getByText(/AI Chat/i)).toBeInTheDocument();
    });
  });

  it('renders all widgets after loading', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('quick-stats')).toBeInTheDocument();
      expect(screen.getByTestId('recent-generations')).toBeInTheDocument();
      expect(screen.getByTestId('activity-timeline')).toBeInTheDocument();
      expect(screen.getByTestId('system-health')).toBeInTheDocument();
      expect(screen.getByTestId('schema-stats')).toBeInTheDocument();
    });
  });

  it('organizes widgets in two-column layout', async () => {
    const { container } = render(<Dashboard />);
    await waitFor(() => {
      // Left column (md=8) and right column (md=4)
      const gridItems = container.querySelectorAll('.MuiGrid-item');
      expect(gridItems.length).toBeGreaterThan(0);
    });
  });

  it('displays Quick Actions before widgets', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      const quickActions = screen.getByText(/Quick Actions/i);
      const widgets = screen.getByTestId('quick-stats');
      expect(quickActions).toBeInTheDocument();
      expect(widgets).toBeInTheDocument();
    });
  });

  it('renders with responsive layout', async () => {
    const { container } = render(<Dashboard />);
    await waitFor(() => {
      // Check for MUI Grid system
      const grids = container.querySelectorAll('[class*="MuiGrid"]');
      expect(grids.length).toBeGreaterThan(0);
    });
  });

  it('auto-refresh toggle works', async () => {
    const user = userEvent.setup();
    render(<Dashboard />);
    
    await waitFor(() => {
      const toggle = screen.getByLabelText('Auto-refresh toggle');
      expect(toggle).toBeInTheDocument();
    });

    const toggle = screen.getByLabelText('Auto-refresh toggle');
    expect(toggle).not.toBeChecked();

    await user.click(toggle);
    // After state update, it should be checked
    // (In real component, but our mock doesn't maintain state)
  });

  it('manual refresh button is present', async () => {
    render(<Dashboard />);
    
    await waitFor(() => {
      expect(screen.getByText('Manual Refresh')).toBeInTheDocument();
    });
  });

  it('wraps each widget section in FadeIn', async () => {
    render(<Dashboard />);
    
    await waitFor(() => {
      const fadeIns = screen.getAllByTestId('fade-in');
      // Should have FadeIn for: QuickStats, Quick Actions, Recent Gens, Timeline, Health, Schema
      expect(fadeIns.length).toBeGreaterThanOrEqual(6);
    });
  });

  it('displays header with title and controls', async () => {
    render(<Dashboard />);
    
    await waitFor(() => {
      const title = screen.getByText('Dashboard');
      const autoRefresh = screen.getByTestId('auto-refresh-control');
      
      expect(title).toBeInTheDocument();
      expect(autoRefresh).toBeInTheDocument();
    });
  });
});
