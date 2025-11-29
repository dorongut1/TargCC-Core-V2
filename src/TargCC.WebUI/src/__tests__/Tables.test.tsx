/**
 * Tables Component Tests
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Tables } from '../pages/Tables';
import { apiService } from '../services/api';

// Mock Pagination component
vi.mock('../components/Pagination', () => ({
  default: ({ total, page, pageSize, onPageChange, onPageSizeChange }: any) => (
    <div data-testid="pagination">
      <button onClick={() => onPageChange(2)}>Page 2</button>
      <button onClick={() => onPageSizeChange(50)}>Size 50</button>
      <div>Total: {total}</div>
    </div>
  )
}));

// Mock FilterMenu component
vi.mock('../components/FilterMenu', () => ({
  default: ({ filters, onFiltersChange }: any) => (
    <div data-testid="filter-menu">
      <button onClick={() => onFiltersChange([{ id: '1', field: 'name', operator: 'equals', value: 'test' }])}>
        Add Filter
      </button>
      <div>Filters: {filters.length}</div>
    </div>
  )
}));

// Mock new components
vi.mock('../components/ErrorBoundary', () => ({
  default: ({ children }: any) => <div data-testid="error-boundary">{children}</div>
}));

vi.mock('../components/TableSkeleton', () => ({
  default: ({ rows, columns }: any) => (
    <tr data-testid="table-skeleton">
      <td colSpan={columns}>Loading {rows} rows...</td>
    </tr>
  )
}));

vi.mock('../components/FadeIn', () => ({
  default: ({ children }: any) => <div data-testid="fade-in">{children}</div>
}));

vi.mock('../components/AutoRefreshControl', () => ({
  default: ({ enabled, onToggle, onManualRefresh }: any) => (
    <div data-testid="auto-refresh-control">
      <input
        type="checkbox"
        checked={enabled}
        onChange={(e) => onToggle(e.target.checked)}
        aria-label="Auto-refresh toggle"
      />
      {onManualRefresh && (
        <button onClick={onManualRefresh}>Manual Refresh</button>
      )}
    </div>
  )
}));

vi.mock('../hooks/useAutoRefresh', () => ({
  useAutoRefresh: () => ({
    lastRefresh: new Date('2024-01-01T12:00:00Z'),
    refresh: vi.fn()
  })
}));

vi.mock('../services/api');

describe('Tables', () => {
  const mockTables = [
    {
      name: 'Customer',
      schema: 'dbo',
      rowCount: 1000,
      generationStatus: 'Generated',
      lastGenerated: new Date('2025-01-01'),
    },
    {
      name: 'Order',
      schema: 'dbo',
      rowCount: 5000,
      generationStatus: 'Not Generated',
      lastGenerated: null,
    },
    {
      name: 'Product',
      schema: 'sales',
      rowCount: 200,
      generationStatus: 'Generated',
      lastGenerated: new Date('2025-01-15'),
    },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    (apiService.getTables as any).mockResolvedValue(mockTables);
  });

  it('wraps content in ErrorBoundary', async () => {
    render(<Tables />);
    await waitFor(() => {
      expect(screen.getByTestId('error-boundary')).toBeInTheDocument();
    });
  });

  it('shows TableSkeleton while loading', () => {
    render(<Tables />);
    expect(screen.getByTestId('table-skeleton')).toBeInTheDocument();
  });

  it('displays AutoRefreshControl in header', async () => {
    render(<Tables />);
    await waitFor(() => {
      expect(screen.getByTestId('auto-refresh-control')).toBeInTheDocument();
    });
  });

  it('renders tables title', async () => {
    render(<Tables />);
    await waitFor(() => {
      expect(screen.getByText(/Database Tables/i)).toBeInTheDocument();
    });
  });

  it('displays search input with FadeIn', async () => {
    render(<Tables />);
    await waitFor(() => {
      expect(screen.getByPlaceholderText(/Search tables/i)).toBeInTheDocument();
    });
  });

  it('displays FilterMenu component with FadeIn', async () => {
    render(<Tables />);
    await waitFor(() => {
      expect(screen.getByTestId('filter-menu')).toBeInTheDocument();
    });
  });

  it('filters tables by search term', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
    });
    
    const searchInput = screen.getByPlaceholderText(/Search tables/i);
    fireEvent.change(searchInput, { target: { value: 'Customer' } });
    
    expect(screen.getByText('Customer')).toBeInTheDocument();
  });

  it('displays table statistics chips with FadeIn', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      expect(screen.getByText(/Total: 3/i)).toBeInTheDocument();
      expect(screen.getByText(/Showing: 3/i)).toBeInTheDocument();
      expect(screen.getByText(/Generated: 2/i)).toBeInTheDocument();
      expect(screen.getByText(/Not Generated: 1/i)).toBeInTheDocument();
    });
  });

  it('renders table headers with sort labels', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      expect(screen.getByText('Name')).toBeInTheDocument();
      expect(screen.getByText('Schema')).toBeInTheDocument();
      expect(screen.getByText('Rows')).toBeInTheDocument();
      expect(screen.getByText('Status')).toBeInTheDocument();
      expect(screen.getByText('Last Generated')).toBeInTheDocument();
    });
  });

  it('displays checkbox column header', async () => {
    const { container } = render(<Tables />);
    
    await waitFor(() => {
      const checkboxes = container.querySelectorAll('input[type="checkbox"]');
      expect(checkboxes.length).toBeGreaterThan(0);
    });
  });

  it('displays table data rows', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
      expect(screen.getByText('Order')).toBeInTheDocument();
      expect(screen.getByText('Product')).toBeInTheDocument();
    });
  });

  it('displays row counts formatted with commas', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      expect(screen.getByText('1,000')).toBeInTheDocument();
      expect(screen.getByText('5,000')).toBeInTheDocument();
    });
  });

  it('displays generation status chips', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      const generatedChips = screen.getAllByText('Generated');
      const notGeneratedChips = screen.getAllByText('Not Generated');
      expect(generatedChips.length).toBe(2);
      expect(notGeneratedChips.length).toBe(1);
    });
  });

  it('displays action buttons for each row', async () => {
    const { container } = render(<Tables />);
    
    await waitFor(() => {
      // Should have Generate, View, Edit buttons for each row
      const buttons = container.querySelectorAll('button[aria-label]');
      expect(buttons.length).toBeGreaterThan(0);
    });
  });

  it('displays Pagination component with FadeIn', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      expect(screen.getByTestId('pagination')).toBeInTheDocument();
    });
  });

  it('handles select all checkbox', async () => {
    const { container } = render(<Tables />);
    
    await waitFor(() => {
      const headerCheckbox = container.querySelector('thead input[type="checkbox"]');
      expect(headerCheckbox).toBeInTheDocument();
    });
  });

  it('shows selected count chip when rows selected', async () => {
    const { container } = render(<Tables />);
    
    await waitFor(() => {
      const rowCheckbox = container.querySelector('tbody input[type="checkbox"]');
      if (rowCheckbox) {
        fireEvent.click(rowCheckbox);
      }
    });
    
    await waitFor(() => {
      expect(screen.getByText(/Selected: 1/i)).toBeInTheDocument();
    });
  });

  it('shows generate button when rows selected', async () => {
    const { container } = render(<Tables />);
    
    await waitFor(() => {
      const rowCheckbox = container.querySelector('tbody input[type="checkbox"]');
      if (rowCheckbox) {
        fireEvent.click(rowCheckbox);
      }
    });
    
    await waitFor(() => {
      expect(screen.getByText(/Generate Selected \(1\)/i)).toBeInTheDocument();
    });
  });

  it('applies filters when filter menu changes', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      const addFilterButton = screen.getByText('Add Filter');
      fireEvent.click(addFilterButton);
    });
    
    // Filter count should update
    expect(screen.getByText(/Filters: 1/i)).toBeInTheDocument();
  });

  it('shows empty state when no tables match search', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      const searchInput = screen.getByPlaceholderText(/Search tables/i);
      fireEvent.change(searchInput, { target: { value: 'NonExistentTable' } });
    });
    
    await waitFor(() => {
      expect(screen.getByText(/No tables found matching your criteria/i)).toBeInTheDocument();
    });
  });

  it('handles sorting when column header clicked', async () => {
    const { container } = render(<Tables />);
    
    await waitFor(() => {
      const nameHeader = screen.getByText('Name');
      fireEvent.click(nameHeader);
    });
    
    // Table should still render after sort
    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
    });
  });

  it('displays last generated dates', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      expect(screen.getByText('1/1/2025')).toBeInTheDocument();
      expect(screen.getByText('1/15/2025')).toBeInTheDocument();
      expect(screen.getByText('Never')).toBeInTheDocument();
    });
  });

  it('handles page changes from pagination', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      const page2Button = screen.getByText('Page 2');
      fireEvent.click(page2Button);
    });
    
    // Table should still be visible after page change
    expect(screen.getByTestId('pagination')).toBeInTheDocument();
  });

  it('handles page size changes from pagination', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      const size50Button = screen.getByText('Size 50');
      fireEvent.click(size50Button);
    });
    
    // Pagination should still be visible
    expect(screen.getByTestId('pagination')).toBeInTheDocument();
  });

  it('auto-refresh toggle works', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      const toggle = screen.getByLabelText('Auto-refresh toggle');
      expect(toggle).toBeInTheDocument();
      expect(toggle).not.toBeChecked();
    });
  });

  it('manual refresh button is present', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      expect(screen.getByText('Manual Refresh')).toBeInTheDocument();
    });
  });

  it('wraps sections in FadeIn components', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      const fadeIns = screen.getAllByTestId('fade-in');
      // Should have FadeIn for: search, filters, stats, table, pagination
      expect(fadeIns.length).toBeGreaterThanOrEqual(5);
    });
  });

  it('displays header with title and auto-refresh control', async () => {
    render(<Tables />);
    
    await waitFor(() => {
      const title = screen.getByText('Database Tables');
      const autoRefresh = screen.getByTestId('auto-refresh-control');
      
      expect(title).toBeInTheDocument();
      expect(autoRefresh).toBeInTheDocument();
    });
  });
});
