/**
 * Header Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Header } from '../components/Header';

describe('Header', () => {
  it('renders TargCC Core V2 title', () => {
    const mockOnMenuClick = vi.fn();
    render(<Header onMenuClick={mockOnMenuClick} />);
    expect(screen.getByText(/TargCC Core V2/i)).toBeInTheDocument();
  });

  it('calls onMenuClick when menu button is clicked', () => {
    const mockOnMenuClick = vi.fn();
    render(<Header onMenuClick={mockOnMenuClick} />);
    const menuButton = screen.getByLabelText(/open drawer/i);
    fireEvent.click(menuButton);
    expect(mockOnMenuClick).toHaveBeenCalledTimes(1);
  });

  it('renders settings and help buttons', () => {
    const mockOnMenuClick = vi.fn();
    render(<Header onMenuClick={mockOnMenuClick} />);
    expect(screen.getByLabelText(/settings/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/help/i)).toBeInTheDocument();
  });

  it('calls onMenuClick multiple times when menu button clicked multiple times', async () => {
    const user = userEvent.setup();
    const mockOnMenuClick = vi.fn();
    render(<Header onMenuClick={mockOnMenuClick} />);
    
    const menuButton = screen.getByLabelText(/open drawer/i);
    await user.click(menuButton);
    await user.click(menuButton);
    await user.click(menuButton);
    
    expect(mockOnMenuClick).toHaveBeenCalledTimes(3);
  });

  it('renders menu icon button', () => {
    const mockOnMenuClick = vi.fn();
    const { container } = render(<Header onMenuClick={mockOnMenuClick} />);
    
    const menuButton = screen.getByLabelText(/open drawer/i);
    expect(menuButton).toBeInTheDocument();
    
    // Should have MenuIcon
    const icons = container.querySelectorAll('svg');
    expect(icons.length).toBeGreaterThan(0);
  });

  it('renders all three icon buttons', () => {
    const mockOnMenuClick = vi.fn();
    render(<Header onMenuClick={mockOnMenuClick} />);
    
    const buttons = screen.getAllByRole('button');
    expect(buttons).toHaveLength(3); // Menu, Help, Settings
  });

  it('renders AppBar component', () => {
    const mockOnMenuClick = vi.fn();
    const { container } = render(<Header onMenuClick={mockOnMenuClick} />);
    
    const appBar = container.querySelector('.MuiAppBar-root');
    expect(appBar).toBeInTheDocument();
  });

  it('positions AppBar as fixed', () => {
    const mockOnMenuClick = vi.fn();
    const { container } = render(<Header onMenuClick={mockOnMenuClick} />);
    
    const appBar = container.querySelector('.MuiAppBar-positionFixed');
    expect(appBar).toBeInTheDocument();
  });

  it('displays title with proper variant', () => {
    const mockOnMenuClick = vi.fn();
    const { container } = render(<Header onMenuClick={mockOnMenuClick} />);
    
    const title = container.querySelector('.MuiTypography-h6');
    expect(title).toBeInTheDocument();
    expect(title?.textContent).toBe('TargCC Core V2');
  });

  it('renders help icon', () => {
    const mockOnMenuClick = vi.fn();
    render(<Header onMenuClick={mockOnMenuClick} />);
    
    const helpButton = screen.getByLabelText(/help/i);
    expect(helpButton).toBeInTheDocument();
  });

  it('renders settings icon', () => {
    const mockOnMenuClick = vi.fn();
    render(<Header onMenuClick={mockOnMenuClick} />);
    
    const settingsButton = screen.getByLabelText(/settings/i);
    expect(settingsButton).toBeInTheDocument();
  });

  it('has proper toolbar structure', () => {
    const mockOnMenuClick = vi.fn();
    const { container } = render(<Header onMenuClick={mockOnMenuClick} />);
    
    const toolbar = container.querySelector('.MuiToolbar-root');
    expect(toolbar).toBeInTheDocument();
  });
});
