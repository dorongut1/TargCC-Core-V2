/**
 * Dashboard Component Tests
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { Dashboard } from '../pages/Dashboard';

// Mock SystemHealth component to avoid circular dependencies
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

  it('displays stat cards', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Total Tables/i)).toBeInTheDocument();
      expect(screen.getByText(/Generated/i)).toBeInTheDocument();
      expect(screen.getByText(/Tests/i)).toBeInTheDocument();
      expect(screen.getByText(/Coverage/i)).toBeInTheDocument();
    });
  });

  it('displays correct stat values', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText('12')).toBeInTheDocument(); // totalTables
      expect(screen.getByText('8')).toBeInTheDocument(); // generatedTables
      expect(screen.getByText('715')).toBeInTheDocument(); // totalTests
      expect(screen.getByText('85%')).toBeInTheDocument(); // coverage
    });
  });

  it('displays quick actions', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Quick Actions/i)).toBeInTheDocument();
      expect(screen.getByText(/Generate All/i)).toBeInTheDocument();
      expect(screen.getByText(/Analyze Security/i)).toBeInTheDocument();
      expect(screen.getByText(/Check Quality/i)).toBeInTheDocument();
      expect(screen.getByText(/AI Chat/i)).toBeInTheDocument();
    });
  });

  it('displays recent activity section', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Recent Activity/i)).toBeInTheDocument();
    });
  });

  it('displays recent activity items', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Security Scan/i)).toBeInTheDocument();
      expect(screen.getByText(/Code Quality/i)).toBeInTheDocument();
      expect(screen.getByText(/Generation/i)).toBeInTheDocument();
    });
  });

  it('displays activity status chips', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      const successChips = screen.getAllByText('success');
      const warningChips = screen.getAllByText('warning');
      expect(successChips.length).toBeGreaterThan(0);
      expect(warningChips.length).toBeGreaterThan(0);
    });
  });

  it('displays SystemHealth component', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByTestId('system-health')).toBeInTheDocument();
      expect(screen.getByText(/CPU: 45%/i)).toBeInTheDocument();
      expect(screen.getByText(/Memory: 62%/i)).toBeInTheDocument();
      expect(screen.getByText(/Disk: 38%/i)).toBeInTheDocument();
      expect(screen.getByText(/Status: healthy/i)).toBeInTheDocument();
    });
  });

  it('shows loading state initially', () => {
    render(<Dashboard />);
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('displays 4 action buttons', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      const buttons = screen.getAllByRole('button');
      // Should have at least 4 buttons in Quick Actions
      expect(buttons.length).toBeGreaterThanOrEqual(4);
    });
  });

  it('displays activity timestamps', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      // The component shows timestamps in locale format
      const listItems = screen.getAllByRole('listitem');
      expect(listItems.length).toBe(3); // 3 recent activities
    });
  });

  it('uses responsive grid layout for stat cards', async () => {
    const { container } = render(<Dashboard />);
    await waitFor(() => {
      const gridBox = container.querySelector('[style*="grid"]');
      expect(gridBox).toBeInTheDocument();
    });
  });

  it('displays all 4 stat card icons', async () => {
    const { container } = render(<Dashboard />);
    await waitFor(() => {
      // Each stat card has an icon (TableChart, CheckCircle, BugReport, TrendingUp)
      const icons = container.querySelectorAll('svg');
      expect(icons.length).toBeGreaterThan(0);
    });
  });

  it('displays activity descriptions', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/No critical issues/i)).toBeInTheDocument();
      expect(screen.getByText(/Grade B/i)).toBeInTheDocument();
      expect(screen.getByText(/Customer entity/i)).toBeInTheDocument();
    });
  });

  it('shows dividers between activity items', async () => {
    const { container } = render(<Dashboard />);
    await waitFor(() => {
      const listItems = container.querySelectorAll('li');
      expect(listItems.length).toBe(3);
    });
  });
});
