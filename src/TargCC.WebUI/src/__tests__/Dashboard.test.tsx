/**
 * Dashboard Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { Dashboard } from '../pages/Dashboard';

describe('Dashboard', () => {
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

  it('displays quick actions', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Quick Actions/i)).toBeInTheDocument();
      expect(screen.getByText(/Generate All/i)).toBeInTheDocument();
    });
  });

  it('displays recent activity section', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/Recent Activity/i)).toBeInTheDocument();
    });
  });

  it('shows loading state initially', () => {
    render(<Dashboard />);
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });
});
