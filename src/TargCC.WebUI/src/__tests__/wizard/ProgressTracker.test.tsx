/**
 * Tests for ProgressTracker component
 * Tests progress display and file tracking functionality
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import ProgressTracker, { ProgressItem } from '../../components/wizard/ProgressTracker';

const mockItems: ProgressItem[] = [
  { id: '1', name: 'CustomerEntity.cs', type: 'entity', status: 'complete' },
  { id: '2', name: 'CustomerRepository.cs', type: 'repository', status: 'processing', message: 'Generating...' },
  { id: '3', name: 'CreateCustomerHandler.cs', type: 'handler', status: 'pending' },
  { id: '4', name: 'CustomerController.cs', type: 'api', status: 'pending' },
];

describe('ProgressTracker', () => {
  describe('Header Display', () => {
    it('renders Generation Progress title', () => {
      render(<ProgressTracker items={mockItems} currentProgress={50} />);
      expect(screen.getByText('Generation Progress')).toBeInTheDocument();
    });

    it('displays completed count correctly', () => {
      render(<ProgressTracker items={mockItems} currentProgress={25} />);
      expect(screen.getByText('1 / 4 files')).toBeInTheDocument();
    });
  });
  describe('Progress Bar', () => {
    it('renders progress bar', () => {
      const { container } = render(<ProgressTracker items={mockItems} currentProgress={50} />);
      const progressBar = container.querySelector('.MuiLinearProgress-root');
      expect(progressBar).toBeInTheDocument();
    });

    it('displays correct progress percentage', () => {
      render(<ProgressTracker items={mockItems} currentProgress={75} />);
      expect(screen.getByText('75% complete')).toBeInTheDocument();
    });
  });

  describe('Progress Items List', () => {
    it('renders all progress items', () => {
      render(<ProgressTracker items={mockItems} currentProgress={50} />);
      
      expect(screen.getByText('CustomerEntity.cs')).toBeInTheDocument();
      expect(screen.getByText('CustomerRepository.cs')).toBeInTheDocument();
      expect(screen.getByText('CreateCustomerHandler.cs')).toBeInTheDocument();
      expect(screen.getByText('CustomerController.cs')).toBeInTheDocument();
    });

    it('displays item messages when provided', () => {
      render(<ProgressTracker items={mockItems} currentProgress={50} />);
      expect(screen.getByText('Generating...')).toBeInTheDocument();
    });
  });
});
