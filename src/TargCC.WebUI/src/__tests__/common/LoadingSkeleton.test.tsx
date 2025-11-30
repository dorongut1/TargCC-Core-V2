/**
 * Tests for LoadingSkeleton component
 * Tests different skeleton types and counts
 */

import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import LoadingSkeleton from '../../components/common/LoadingSkeleton';

describe('LoadingSkeleton', () => {
  describe('Card Type (Default)', () => {
    it('renders card skeleton by default', () => {
      const { container } = render(<LoadingSkeleton />);
      const skeletons = container.querySelectorAll('.MuiSkeleton-root');
      expect(skeletons.length).toBeGreaterThan(0);
    });

    it('renders default count of 3 cards', () => {
      const { container } = render(<LoadingSkeleton type="card" />);
      const papers = container.querySelectorAll('.MuiPaper-root');
      expect(papers.length).toBe(3);
    });
  });

  describe('Table Type', () => {
    it('renders table skeleton', () => {
      const { container } = render(<LoadingSkeleton type="table" />);
      const paper = container.querySelector('.MuiPaper-root');
      expect(paper).toBeInTheDocument();
    });
  });
});
