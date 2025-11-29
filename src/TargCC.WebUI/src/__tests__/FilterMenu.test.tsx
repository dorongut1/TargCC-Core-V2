import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import FilterMenu from '../components/FilterMenu';

describe('FilterMenu', () => {
  const mockOnFiltersChange = vi.fn();
  const availableFields = [
    { value: 'name', label: 'Name' },
    { value: 'status', label: 'Status' },
    { value: 'count', label: 'Count' }
  ];

  beforeEach(() => {
    mockOnFiltersChange.mockClear();
  });

  it('renders filter button', () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    expect(screen.getByRole('button', { name: /Filters/i })).toBeInTheDocument();
  });

  it('shows filter count when filters exist', () => {
    const filters = [
      { id: '1', field: 'name', operator: 'equals' as const, value: 'test' }
    ];
    
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={filters}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    expect(screen.getByText(/Filters \(1\)/i)).toBeInTheDocument();
  });

  it('shows Clear All button when filters exist', () => {
    const filters = [
      { id: '1', field: 'name', operator: 'equals' as const, value: 'test' }
    ];
    
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={filters}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    expect(screen.getByText('Clear All')).toBeInTheDocument();
  });

  it('does not show Clear All button when no filters', () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    expect(screen.queryByText('Clear All')).not.toBeInTheDocument();
  });

  it('displays filter chips for active filters', () => {
    const filters = [
      { id: '1', field: 'name', operator: 'equals' as const, value: 'test' }
    ];
    
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={filters}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    expect(screen.getByText(/Name Equals "test"/i)).toBeInTheDocument();
  });

  it('calls onFiltersChange with empty array when Clear All clicked', () => {
    const filters = [
      { id: '1', field: 'name', operator: 'equals' as const, value: 'test' }
    ];
    
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={filters}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const clearButton = screen.getByText('Clear All');
    fireEvent.click(clearButton);
    
    expect(mockOnFiltersChange).toHaveBeenCalledWith([]);
  });

  it('removes individual filter when chip delete clicked', () => {
    const filters = [
      { id: '1', field: 'name', operator: 'equals' as const, value: 'test' },
      { id: '2', field: 'status', operator: 'contains' as const, value: 'active' }
    ];
    
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={filters}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    // Find delete button (MUI chip uses SVG for delete)
    const deleteButtons = screen.getAllByTestId('CancelIcon');
    fireEvent.click(deleteButtons[0]);
    
    expect(mockOnFiltersChange).toHaveBeenCalledWith([filters[1]]);
  });

  it('opens popover when filter button clicked', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      expect(screen.getByText('Add Filter')).toBeInTheDocument();
    });
  });

  it('shows field selector in popover', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      expect(screen.getByLabelText('Field')).toBeInTheDocument();
    });
  });

  it('shows operator selector in popover', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      expect(screen.getByLabelText('Operator')).toBeInTheDocument();
    });
  });

  it('shows value input in popover', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      expect(screen.getByLabelText('Value')).toBeInTheDocument();
    });
  });

  it('disables Add Filter button when field not selected', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      const addButton = screen.getByRole('button', { name: /Add Filter/i });
      expect(addButton).toBeDisabled();
    });
  });

  it('disables Add Filter button when value is empty', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      const fieldSelect = screen.getByLabelText('Field');
      fireEvent.mouseDown(fieldSelect);
    });
    
    const nameOption = screen.getByRole('option', { name: 'Name' });
    fireEvent.click(nameOption);
    
    const addButton = screen.getByRole('button', { name: /Add Filter/i });
    expect(addButton).toBeDisabled();
  });

  it('closes popover when Cancel clicked', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      expect(screen.getByText('Add Filter')).toBeInTheDocument();
    });
    
    const cancelButton = screen.getByRole('button', { name: /Cancel/i });
    fireEvent.click(cancelButton);
    
    await waitFor(() => {
      expect(screen.queryByText('Add Filter')).not.toBeInTheDocument();
    });
  });

  it('displays all available fields in dropdown', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      const fieldSelect = screen.getByLabelText('Field');
      fireEvent.mouseDown(fieldSelect);
    });
    
    expect(screen.getByRole('option', { name: 'Name' })).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Status' })).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Count' })).toBeInTheDocument();
  });

  it('displays all operator options', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      const operatorSelect = screen.getByLabelText('Operator');
      fireEvent.mouseDown(operatorSelect);
    });
    
    expect(screen.getByRole('option', { name: 'Equals' })).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Contains' })).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Greater than' })).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Less than' })).toBeInTheDocument();
  });

  it('resets form after adding filter', async () => {
    render(
      <FilterMenu
        availableFields={availableFields}
        filters={[]}
        onFiltersChange={mockOnFiltersChange}
      />
    );
    
    const filterButton = screen.getByRole('button', { name: /Filters/i });
    fireEvent.click(filterButton);
    
    await waitFor(() => {
      const fieldSelect = screen.getByLabelText('Field');
      fireEvent.mouseDown(fieldSelect);
    });
    
    const nameOption = screen.getByRole('option', { name: 'Name' });
    fireEvent.click(nameOption);
    
    const valueInput = screen.getByLabelText('Value');
    fireEvent.change(valueInput, { target: { value: 'test' } });
    
    const addButton = screen.getByRole('button', { name: /Add Filter/i });
    fireEvent.click(addButton);
    
    // Form should be reset - value input should be empty
    await waitFor(() => {
      const newValueInput = screen.getByLabelText('Value') as HTMLInputElement;
      expect(newValueInput.value).toBe('');
    });
  });
});
