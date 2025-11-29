/**
 * Header Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
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
});
