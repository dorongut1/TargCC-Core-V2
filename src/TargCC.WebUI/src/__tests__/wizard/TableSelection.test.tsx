import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import TableSelection from '../../components/wizard/TableSelection';
import type { WizardData } from '../../components/wizard/GenerationWizard';

describe('TableSelection', () => {
  const mockData: WizardData = {
    selectedTables: [],
    options: {
      entities: true,
      repositories: true,
      handlers: true,
      api: true
    }
  };

  const mockOnChange = vi.fn();

  it('renders the component with title', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);
    expect(screen.getByText('Select Tables to Generate')).toBeInTheDocument();
  });

  it('renders all table options', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);
    expect(screen.getByText('Customer')).toBeInTheDocument();
    expect(screen.getByText('Order')).toBeInTheDocument();
    expect(screen.getByText('Product')).toBeInTheDocument();
    expect(screen.getByText('Employee')).toBeInTheDocument();
  });

  it('renders search input', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);
    const searchInput = screen.getByPlaceholderText('Search tables...');
    expect(searchInput).toBeInTheDocument();
  });

  it('filters tables based on search term', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);

    const searchInput = screen.getByPlaceholderText('Search tables...');
    fireEvent.change(searchInput, { target: { value: 'Cust' } });

    expect(screen.getByText('Customer')).toBeInTheDocument();
    expect(screen.queryByText('Order')).not.toBeInTheDocument();
    expect(screen.queryByText('Product')).not.toBeInTheDocument();
  });

  it('shows "No tables found" when search returns no results', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);

    const searchInput = screen.getByPlaceholderText('Search tables...');
    fireEvent.change(searchInput, { target: { value: 'NonExistent' } });

    expect(screen.getByText('No tables found')).toBeInTheDocument();
  });

  it('calls onChange when a table is selected', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);

    const customerCheckbox = screen.getByRole('checkbox', { name: /customer/i });
    fireEvent.click(customerCheckbox);

    expect(mockOnChange).toHaveBeenCalledWith({
      ...mockData,
      selectedTables: ['Customer']
    });
  });

  it('calls onChange when a table is deselected', () => {
    const dataWithSelection = {
      ...mockData,
      selectedTables: ['Customer', 'Order']
    };

    render(<TableSelection data={dataWithSelection} onChange={mockOnChange} />);

    const customerCheckbox = screen.getByRole('checkbox', { name: /customer/i });
    fireEvent.click(customerCheckbox);

    expect(mockOnChange).toHaveBeenCalledWith({
      ...mockData,
      selectedTables: ['Order']
    });
  });

  it('Select All button selects all filtered tables', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);

    const selectAllButton = screen.getByRole('button', { name: /select all/i });
    fireEvent.click(selectAllButton);

    expect(mockOnChange).toHaveBeenCalledWith({
      ...mockData,
      selectedTables: expect.arrayContaining(['Customer', 'Order', 'Product', 'Employee'])
    });
  });

  it('Select All only selects filtered tables when search is active', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);

    // Search for 'Cust'
    const searchInput = screen.getByPlaceholderText('Search tables...');
    fireEvent.change(searchInput, { target: { value: 'Cust' } });

    // Click Select All
    const selectAllButton = screen.getByRole('button', { name: /select all/i });
    fireEvent.click(selectAllButton);

    expect(mockOnChange).toHaveBeenCalledWith({
      ...mockData,
      selectedTables: ['Customer']
    });
  });

  it('Select None button clears all selections', () => {
    const dataWithSelection = {
      ...mockData,
      selectedTables: ['Customer', 'Order']
    };

    render(<TableSelection data={dataWithSelection} onChange={mockOnChange} />);

    const selectNoneButton = screen.getByRole('button', { name: /select none/i });
    fireEvent.click(selectNoneButton);

    expect(mockOnChange).toHaveBeenCalledWith({
      ...mockData,
      selectedTables: []
    });
  });

  it('displays correct selection count', () => {
    const dataWithSelection = {
      ...mockData,
      selectedTables: ['Customer', 'Order', 'Product']
    };

    render(<TableSelection data={dataWithSelection} onChange={mockOnChange} />);

    expect(screen.getByText('3 table(s) selected')).toBeInTheDocument();
  });

  it('displays zero selection count initially', () => {
    render(<TableSelection data={mockData} onChange={mockOnChange} />);

    expect(screen.getByText('0 table(s) selected')).toBeInTheDocument();
  });
});
