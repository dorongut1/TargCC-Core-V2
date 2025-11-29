import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import AutoRefreshControl from '../components/AutoRefreshControl';

describe('AutoRefreshControl', () => {
  const defaultProps = {
    enabled: false,
    onToggle: vi.fn(),
    lastRefresh: new Date(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders without crashing', () => {
    render(<AutoRefreshControl {...defaultProps} />);
    expect(screen.getByText('Auto-refresh')).toBeInTheDocument();
  });

  it('renders toggle switch', () => {
    render(<AutoRefreshControl {...defaultProps} />);
    
    const switchElement = screen.getByRole('checkbox');
    expect(switchElement).toBeInTheDocument();
  });

  it('switch is unchecked when disabled', () => {
    render(<AutoRefreshControl {...defaultProps} enabled={false} />);
    
    const switchElement = screen.getByRole('checkbox');
    expect(switchElement).not.toBeChecked();
  });

  it('switch is checked when enabled', () => {
    render(<AutoRefreshControl {...defaultProps} enabled={true} />);
    
    const switchElement = screen.getByRole('checkbox');
    expect(switchElement).toBeChecked();
  });

  it('calls onToggle when switch is clicked', async () => {
    const user = userEvent.setup();
    const onToggle = vi.fn();
    
    render(<AutoRefreshControl {...defaultProps} onToggle={onToggle} />);
    
    const switchElement = screen.getByRole('checkbox');
    await user.click(switchElement);
    
    expect(onToggle).toHaveBeenCalledWith(true);
  });

  it('does not show chip when disabled', () => {
    render(<AutoRefreshControl {...defaultProps} enabled={false} />);
    
    expect(screen.queryByText(/Updated/)).not.toBeInTheDocument();
  });

  it('shows chip with last refresh time when enabled', () => {
    render(<AutoRefreshControl {...defaultProps} enabled={true} />);
    
    expect(screen.getByText(/Updated/)).toBeInTheDocument();
  });

  it('displays "just now" for very recent refresh', () => {
    const justNow = new Date();
    render(
      <AutoRefreshControl
        {...defaultProps}
        enabled={true}
        lastRefresh={justNow}
      />
    );
    
    expect(screen.getByText(/just now/)).toBeInTheDocument();
  });

  it('displays seconds ago for recent refresh', () => {
    const thirtySecondsAgo = new Date(Date.now() - 30000);
    render(
      <AutoRefreshControl
        {...defaultProps}
        enabled={true}
        lastRefresh={thirtySecondsAgo}
      />
    );
    
    expect(screen.getByText(/30s ago/)).toBeInTheDocument();
  });

  it('displays minutes ago for older refresh', () => {
    const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
    render(
      <AutoRefreshControl
        {...defaultProps}
        enabled={true}
        lastRefresh={fiveMinutesAgo}
      />
    );
    
    expect(screen.getByText(/5m ago/)).toBeInTheDocument();
  });

  it('displays hours ago for much older refresh', () => {
    const twoHoursAgo = new Date(Date.now() - 2 * 60 * 60 * 1000);
    render(
      <AutoRefreshControl
        {...defaultProps}
        enabled={true}
        lastRefresh={twoHoursAgo}
      />
    );
    
    expect(screen.getByText(/2h ago/)).toBeInTheDocument();
  });

  it('displays days ago for very old refresh', () => {
    const threeDaysAgo = new Date(Date.now() - 3 * 24 * 60 * 60 * 1000);
    render(
      <AutoRefreshControl
        {...defaultProps}
        enabled={true}
        lastRefresh={threeDaysAgo}
      />
    );
    
    expect(screen.getByText(/3d ago/)).toBeInTheDocument();
  });

  it('shows manual refresh button when callback provided', () => {
    const onManualRefresh = vi.fn();
    render(
      <AutoRefreshControl
        {...defaultProps}
        onManualRefresh={onManualRefresh}
      />
    );
    
    const refreshButton = screen.getByRole('button', { name: /refresh now/i });
    expect(refreshButton).toBeInTheDocument();
  });

  it('does not show manual refresh button when callback not provided', () => {
    render(<AutoRefreshControl {...defaultProps} />);
    
    const refreshButton = screen.queryByRole('button', { name: /refresh now/i });
    expect(refreshButton).not.toBeInTheDocument();
  });

  it('calls onManualRefresh when refresh button clicked', async () => {
    const user = userEvent.setup();
    const onManualRefresh = vi.fn();
    
    render(
      <AutoRefreshControl
        {...defaultProps}
        onManualRefresh={onManualRefresh}
      />
    );
    
    const refreshButton = screen.getByRole('button', { name: /refresh now/i });
    await user.click(refreshButton);
    
    expect(onManualRefresh).toHaveBeenCalledTimes(1);
  });

  it('chip shows refresh icon', () => {
    render(<AutoRefreshControl {...defaultProps} enabled={true} />);
    
    const chip = screen.getByText(/Updated/).parentElement;
    expect(chip).toBeInTheDocument();
    
    const icon = chip?.querySelector('svg');
    expect(icon).toBeInTheDocument();
  });

  it('manual refresh button shows refresh icon', () => {
    const onManualRefresh = vi.fn();
    render(
      <AutoRefreshControl
        {...defaultProps}
        onManualRefresh={onManualRefresh}
      />
    );
    
    const button = screen.getByRole('button', { name: /refresh now/i });
    const icon = button.querySelector('svg');
    expect(icon).toBeInTheDocument();
  });
});
