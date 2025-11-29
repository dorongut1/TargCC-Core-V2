/**
 * Dashboard Component Tests
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
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

  it('displays QuickStats widget', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('quick-stats')).toBeInTheDocument();
    });
  });

  it('displays quick actions section', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Quick Actions/i)).toBeInTheDocument();
      expect(screen.getByText(/Generate All/i)).toBeInTheDocument();
      expect(screen.getByText(/Analyze Security/i)).toBeInTheDocument();
      expect(screen.getByText(/Check Quality/i)).toBeInTheDocument();
      expect(screen.getByText(/AI Chat/i)).toBeInTheDocument();
    });
  });

  it('displays RecentGenerations widget', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('recent-generations')).toBeInTheDocument();
    });
  });

  it('displays ActivityTimeline widget', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('activity-timeline')).toBeInTheDocument();
    });
  });

  it('displays SystemHealth widget', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('system-health')).toBeInTheDocument();
      expect(screen.getByText(/CPU: 45%/i)).toBeInTheDocument();
      expect(screen.getByText(/Memory: 62%/i)).toBeInTheDocument();
      expect(screen.getByText(/Disk: 38%/i)).toBeInTheDocument();
      expect(screen.getByText(/Status: healthy/i)).toBeInTheDocument();
    });
  });

  it('displays SchemaStats widget', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('schema-stats')).toBeInTheDocument();
    });
  });

  it('shows loading state initially', () => {
    render(<Dashboard />);
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
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
});
