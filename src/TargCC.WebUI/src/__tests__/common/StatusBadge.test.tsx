/**
 * Tests for StatusBadge component
 * Tests status indicator display and configurations
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import StatusBadge from '../../components/common/StatusBadge';

describe('StatusBadge', () => {
  describe('Status Types', () => {
    it('renders success status with default label', () => {
      render(<StatusBadge status="success" />);
      expect(screen.getByText('Complete')).toBeInTheDocument();
    });

    it('renders error status with default label', () => {
      render(<StatusBadge status="error" />);
      expect(screen.getByText('Error')).toBeInTheDocument();
    });

    it('renders pending status with default label', () => {
      render(<StatusBadge status="pending" />);
      expect(screen.getByText('Pending')).toBeInTheDocument();
    });

    it('renders processing status with default label', () => {
      render(<StatusBadge status="processing" />);
      expect(screen.getByText('Processing')).toBeInTheDocument();
    });
  });
  describe('Custom Labels', () => {
    it('renders custom label for success', () => {
      render(<StatusBadge status="success" label="Done!" />);
      expect(screen.getByText('Done!')).toBeInTheDocument();
    });
  });

  describe('Icons', () => {
    it('includes an icon for success status', () => {
      const { container } = render(<StatusBadge status="success" />);
      const icon = container.querySelector('svg');
      expect(icon).toBeInTheDocument();
    });
  });
});
