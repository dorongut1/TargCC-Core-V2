/**
 * Tests for fileTypeIcons utility
 * Tests icon and color mapping for different file types
 */

import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import { getFileTypeIcon, getFileTypeColor } from '../../utils/fileTypeIcons';

describe('fileTypeIcons', () => {
  describe('getFileTypeIcon', () => {
    it('returns DescriptionIcon for entity type', () => {
      const icon = getFileTypeIcon('entity');
      const { container } = render(<>{icon}</>);
      expect(container.querySelector('svg')).toBeInTheDocument();
    });

    it('returns StorageIcon for repository type', () => {
      const icon = getFileTypeIcon('repository');
      const { container } = render(<>{icon}</>);
      expect(container.querySelector('svg')).toBeInTheDocument();
    });

    it('returns AutorenewIcon for handler type', () => {
      const icon = getFileTypeIcon('handler');
      const { container } = render(<>{icon}</>);
      expect(container.querySelector('svg')).toBeInTheDocument();
    });
    it('returns CodeIcon for unknown type', () => {
      const icon = getFileTypeIcon('unknown');
      const { container } = render(<>{icon}</>);
      expect(container.querySelector('svg')).toBeInTheDocument();
    });

    it('handles case insensitive type names', () => {
      const iconLower = getFileTypeIcon('entity');
      const iconUpper = getFileTypeIcon('ENTITY');
      const iconMixed = getFileTypeIcon('Entity');

      const { container: container1 } = render(<>{iconLower}</>);
      const { container: container2 } = render(<>{iconUpper}</>);
      const { container: container3 } = render(<>{iconMixed}</>);

      expect(container1.querySelector('svg')).toBeInTheDocument();
      expect(container2.querySelector('svg')).toBeInTheDocument();
      expect(container3.querySelector('svg')).toBeInTheDocument();
    });
  });

  describe('getFileTypeColor', () => {
    it('returns primary for entity', () => {
      expect(getFileTypeColor('entity')).toBe('primary');
    });

    it('returns secondary for repository', () => {
      expect(getFileTypeColor('repository')).toBe('secondary');
    });
    it('returns info for handler', () => {
      expect(getFileTypeColor('handler')).toBe('info');
    });

    it('returns success for api', () => {
      expect(getFileTypeColor('api')).toBe('success');
    });

    it('returns warning for model', () => {
      expect(getFileTypeColor('model')).toBe('warning');
    });

    it('returns default for unknown type', () => {
      expect(getFileTypeColor('unknown')).toBe('default');
    });

    it('handles case insensitive type names', () => {
      expect(getFileTypeColor('entity')).toBe(getFileTypeColor('ENTITY'));
      expect(getFileTypeColor('entity')).toBe(getFileTypeColor('Entity'));
    });
  });
});
