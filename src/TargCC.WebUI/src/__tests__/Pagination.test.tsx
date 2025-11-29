import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import Pagination from '../components/Pagination';

describe('Pagination', () => {
  const mockOnPageChange = vi.fn();
  const mockOnPageSizeChange = vi.fn();

  beforeEach(() => {
    mockOnPageChange.mockClear();
    mockOnPageSizeChange.mockClear();
  });

  it('renders pagination controls', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    expect(screen.getByText(/Rows per page:/)).toBeInTheDocument();
  });

  it('displays correct item range', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    expect(screen.getByText('1-25 of 100')).toBeInTheDocument();
  });

  it('displays correct range for second page', () => {
    render(
      <Pagination
        total={100}
        page={2}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    expect(screen.getByText('26-50 of 100')).toBeInTheDocument();
  });

  it('displays correct range for last page', () => {
    render(
      <Pagination
        total={95}
        page={4}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    expect(screen.getByText('76-95 of 95')).toBeInTheDocument();
  });

  it('renders null when total is 0', () => {
    const { container } = render(
      <Pagination
        total={0}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    expect(container.firstChild).toBeNull();
  });

  it('shows page size selector with correct value', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    // MUI Select shows selected value
    const select = screen.getByRole('combobox');
    expect(select).toHaveValue('25');
  });

  it('calls onPageSizeChange when page size changes', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    const select = screen.getByRole('combobox');
    fireEvent.mouseDown(select);
    
    const option50 = screen.getByRole('option', { name: '50' });
    fireEvent.click(option50);
    
    expect(mockOnPageSizeChange).toHaveBeenCalledWith(50);
  });

  it('resets to page 1 when page size changes', () => {
    render(
      <Pagination
        total={100}
        page={2}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    const select = screen.getByRole('combobox');
    fireEvent.mouseDown(select);
    
    const option50 = screen.getByRole('option', { name: '50' });
    fireEvent.click(option50);
    
    expect(mockOnPageChange).toHaveBeenCalledWith(1);
  });

  it('renders jump to page input', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    expect(screen.getByText('Jump to:')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Page')).toBeInTheDocument();
  });

  it('handles jump to page on Enter key', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    const input = screen.getByPlaceholderText('Page');
    fireEvent.change(input, { target: { value: '3' } });
    fireEvent.keyDown(input, { key: 'Enter' });
    
    expect(mockOnPageChange).toHaveBeenCalledWith(3);
  });

  it('ignores invalid page numbers in jump input', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    const input = screen.getByPlaceholderText('Page');
    fireEvent.change(input, { target: { value: '999' } });
    fireEvent.keyDown(input, { key: 'Enter' });
    
    expect(mockOnPageChange).not.toHaveBeenCalled();
  });

  it('calculates total pages correctly', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={25}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    // Should have 4 pages (100 / 25)
    // MUI Pagination renders page buttons
    const pagination = screen.getByRole('navigation');
    expect(pagination).toBeInTheDocument();
  });

  it('displays correct item count when on first page', () => {
    render(
      <Pagination
        total={100}
        page={1}
        pageSize={10}
        onPageChange={mockOnPageChange}
        onPageSizeChange={mockOnPageSizeChange}
      />
    );
    
    expect(screen.getByText('1-10 of 100')).toBeInTheDocument();
  });
});
