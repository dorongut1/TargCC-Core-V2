/**
 * Tables Component Tests
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { Tables } from '../pages/Tables';
import { apiService } from '../services/api';
import type { Table } from '../types/models';

// Mock API service
vi.mock('../services/api', () => ({
  apiService: {
    getTables: vi.fn(),
    generateCode: vi.fn(),
  },
}));

// Mock table data
const mockTables: Table[] = [
  {
    name: 'Customer',
    schema: 'dbo',
    columns: [],
    foreignKeys: [],
    isGenerated: true,
    generationStatus: 'Generated',
    rowCount: 1500,
    lastGenerated: new Date('2025-01-15'),
  },
  {
    name: 'Order',
    schema: 'dbo',
    columns: [],
    foreignKeys: [],
    isGenerated: false,
    generationStatus: 'Not Generated',
    rowCount: 3200,
  },
  {
    name: 'Product',
    schema: 'dbo',
    columns: [],
    foreignKeys: [],
    isGenerated: true,
    generationStatus: 'Generated',
    rowCount: 850,
    lastGenerated: new Date('2025-01-14'),
  },
];

describe('Tables Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders tables page title', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Database Tables')).toBeInTheDocument();
    });
  });

  it('displays loading state initially', () => {
    vi.mocked(apiService.getTables).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('displays tables after loading', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
      expect(screen.getByText('Order')).toBeInTheDocument();
      expect(screen.getByText('Product')).toBeInTheDocument();
    });
  });

  it('displays table status chips', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      const generatedChips = screen.getAllByText('Generated');
      expect(generatedChips).toHaveLength(2);
      expect(screen.getByText('Not Generated')).toBeInTheDocument();
    });
  });

  it('displays search field', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByPlaceholderText('Search tables...')).toBeInTheDocument();
    });
  });

  it('displays stats chips', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Total: 3/)).toBeInTheDocument();
      expect(screen.getByText(/Generated: 2/)).toBeInTheDocument();
      expect(screen.getByText(/Not Generated: 1/)).toBeInTheDocument();
    });
  });

  it('displays refresh button', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Refresh')).toBeInTheDocument();
    });
  });

  it('handles error state', async () => {
    vi.mocked(apiService.getTables).mockRejectedValue(new Error('Failed to load'));

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Failed to load/)).toBeInTheDocument();
    });
  });

  it('displays table row count', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('1,500')).toBeInTheDocument();
      expect(screen.getByText('3,200')).toBeInTheDocument();
      expect(screen.getByText('850')).toBeInTheDocument();
    });
  });

  it('displays last generated date', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/1\/15\/2025/)).toBeInTheDocument();
      expect(screen.getByText(/1\/14\/2025/)).toBeInTheDocument();
      expect(screen.getByText('Never')).toBeInTheDocument();
    });
  });

  it('filters tables by search term', async () => {
    const user = userEvent.setup();
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('Search tables...');
    await user.type(searchInput, 'Cust');

    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
      expect(screen.queryByText('Order')).not.toBeInTheDocument();
      expect(screen.queryByText('Product')).not.toBeInTheDocument();
    });
  });

  it('filters tables by schema name', async () => {
    const user = userEvent.setup();
    const tablesWithDifferentSchemas: Table[] = [
      { ...mockTables[0], schema: 'dbo' },
      { ...mockTables[1], schema: 'sales' },
    ];
    vi.mocked(apiService.getTables).mockResolvedValue(tablesWithDifferentSchemas);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('dbo')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('Search tables...');
    await user.type(searchInput, 'sales');

    await waitFor(() => {
      expect(screen.getByText('sales')).toBeInTheDocument();
      const dboElements = screen.queryAllByText('dbo');
      expect(dboElements).toHaveLength(0);
    });
  });

  it('shows no results message when search has no matches', async () => {
    const user = userEvent.setup();
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('Search tables...');
    await user.type(searchInput, 'NonExistent');

    await waitFor(() => {
      expect(screen.getByText(/No tables found matching your search/i)).toBeInTheDocument();
    });
  });

  it('calls refresh when refresh button is clicked', async () => {
    const user = userEvent.setup();
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Refresh')).toBeInTheDocument();
    });

    // Clear the initial call
    vi.clearAllMocks();

    const refreshButton = screen.getByText('Refresh');
    await user.click(refreshButton);

    expect(apiService.getTables).toHaveBeenCalledTimes(1);
  });

  it('calls retry when retry button is clicked in error state', async () => {
    const user = userEvent.setup();
    vi.mocked(apiService.getTables).mockRejectedValue(new Error('Failed to load'));

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Failed to load/)).toBeInTheDocument();
    });

    vi.clearAllMocks();
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    const retryButton = screen.getByText('Retry');
    await user.click(retryButton);

    expect(apiService.getTables).toHaveBeenCalledTimes(1);
  });

  it('calls generateCode when generate button is clicked', async () => {
    const user = userEvent.setup();
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);
    vi.mocked(apiService.generateCode).mockResolvedValue(undefined);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
    });

    const generateButtons = screen.getAllByTitle('Generate Code');
    await user.click(generateButtons[0]);

    expect(apiService.generateCode).toHaveBeenCalledWith({
      tableName: 'Customer',
      options: {
        generateEntity: true,
        generateRepository: true,
        generateService: true,
        generateController: true,
        generateTests: true,
        overwriteExisting: false,
      },
    });
  });

  it('reloads tables after successful generation', async () => {
    const user = userEvent.setup();
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);
    vi.mocked(apiService.generateCode).mockResolvedValue(undefined);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Customer')).toBeInTheDocument();
    });

    vi.clearAllMocks();

    const generateButtons = screen.getAllByTitle('Generate Code');
    await user.click(generateButtons[0]);

    await waitFor(() => {
      expect(apiService.getTables).toHaveBeenCalledTimes(1);
    });
  });

  it('displays action buttons for each table', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      // Each table should have 3 action buttons
      expect(screen.getAllByTitle('Generate Code')).toHaveLength(3);
      expect(screen.getAllByTitle('View Details')).toHaveLength(3);
      expect(screen.getAllByTitle('Edit Configuration')).toHaveLength(3);
    });
  });

  it('updates filtered count display', async () => {
    const user = userEvent.setup();
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Showing 3 of 3 tables/i)).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('Search tables...');
    await user.type(searchInput, 'Cust');

    await waitFor(() => {
      expect(screen.getByText(/Showing 1 of 3 tables/i)).toBeInTheDocument();
    });
  });

  it('displays all table schemas correctly', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue(mockTables);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      const schemaElements = screen.getAllByText('dbo');
      expect(schemaElements).toHaveLength(3);
    });
  });

  it('shows empty state when no tables exist', async () => {
    vi.mocked(apiService.getTables).mockResolvedValue([]);

    render(
      <BrowserRouter>
        <Tables />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/No tables available/i)).toBeInTheDocument();
    });
  });
});
