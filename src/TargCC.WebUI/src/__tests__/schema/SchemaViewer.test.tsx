import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import SchemaViewer from '../../components/schema/SchemaViewer';
import type { DatabaseSchema } from '../../types/schema';

/**
 * Test suite for SchemaViewer component
 */
describe.skip('SchemaViewer', () => {
  // Mock schema for testing
  const mockSchema: DatabaseSchema = {
    tables: [
      {
        name: 'Customer',
        schema: 'dbo',
        rowCount: 100,
        hasTargCCColumns: true,
        columns: [
          {
            name: 'CustomerId',
            type: 'int',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false,
          },
          {
            name: 'eno_FirstName',
            type: 'nvarchar',
            nullable: false,
            isPrimaryKey: false,
            isForeignKey: false,
          },
        ],
      },
      {
        name: 'Order',
        schema: 'dbo',
        rowCount: 500,
        hasTargCCColumns: false,
        columns: [
          {
            name: 'OrderId',
            type: 'int',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false,
          },
        ],
      },
      {
        name: 'Product',
        schema: 'dbo',
        hasTargCCColumns: true,
        columns: [
          {
            name: 'ProductId',
            type: 'int',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false,
          },
        ],
      },
    ],
    relationships: [
      {
        fromTable: 'Order',
        fromColumn: 'CustomerId',
        toTable: 'Customer',
        toColumn: 'CustomerId',
        type: 'one-to-many',
      },
    ],
  };

  it('should render database schema header', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText('Database Schema')).toBeInTheDocument();
  });

  it('should show total table count', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText('3 tables')).toBeInTheDocument();
  });

  it('should show TargCC table count', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText('2 TargCC')).toBeInTheDocument();
  });

  it('should render search input', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByPlaceholderText('Search tables and columns...')).toBeInTheDocument();
  });

  it('should filter tables by name', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const searchInput = screen.getByPlaceholderText('Search tables and columns...');

    fireEvent.change(searchInput, { target: { value: 'Customer' } });

    expect(screen.getByText('Customer')).toBeInTheDocument();
    expect(screen.queryByText('Order')).not.toBeInTheDocument();
  });

  it('should filter tables by column name', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const searchInput = screen.getByPlaceholderText('Search tables and columns...');

    fireEvent.change(searchInput, { target: { value: 'eno_FirstName' } });

    expect(screen.getByText('Customer')).toBeInTheDocument();
    expect(screen.queryByText('Order')).not.toBeInTheDocument();
  });

  it('should show empty state when no tables match search', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const searchInput = screen.getByPlaceholderText('Search tables and columns...');

    fireEvent.change(searchInput, { target: { value: 'NonExistentTable' } });

    expect(screen.getByText('No tables found')).toBeInTheDocument();
  });

  it('should render TargCC filter chip', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText('TargCC Only')).toBeInTheDocument();
  });

  it('should render relationships filter chip', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText('With Relationships')).toBeInTheDocument();
  });

  it('should filter by TargCC tables when TargCC filter clicked', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const targccFilter = screen.getByText('TargCC Only');

    fireEvent.click(targccFilter);

    // Should show Customer and Product (both have TargCC)
    expect(screen.getByText('Customer')).toBeInTheDocument();
    expect(screen.getByText('Product')).toBeInTheDocument();
    // Should not show Order (no TargCC)
    expect(screen.queryByText('Order')).not.toBeInTheDocument();
  });

  it('should filter by tables with relationships when filter clicked', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const relationshipsFilter = screen.getByText('With Relationships');

    fireEvent.click(relationshipsFilter);

    // Should show Customer and Order (both have relationships)
    expect(screen.getByText('Customer')).toBeInTheDocument();
    expect(screen.getByText('Order')).toBeInTheDocument();
    // Should not show Product (no relationships)
    expect(screen.queryByText('Product')).not.toBeInTheDocument();
  });

  it('should show Clear Filters chip when filters are active', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const targccFilter = screen.getByText('TargCC Only');

    fireEvent.click(targccFilter);

    expect(screen.getByText('Clear Filters')).toBeInTheDocument();
  });

  it('should clear all filters when Clear Filters clicked', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const targccFilter = screen.getByText('TargCC Only');

    fireEvent.click(targccFilter);
    expect(screen.getByText('Clear Filters')).toBeInTheDocument();

    const clearButton = screen.getByText('Clear Filters');
    fireEvent.click(clearButton);

    // All tables should be visible again
    expect(screen.getByText('Customer')).toBeInTheDocument();
    expect(screen.getByText('Order')).toBeInTheDocument();
    expect(screen.getByText('Product')).toBeInTheDocument();
  });

  it('should combine search and filters', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const searchInput = screen.getByPlaceholderText('Search tables and columns...');
    const targccFilter = screen.getByText('TargCC Only');

    fireEvent.change(searchInput, { target: { value: 'Cust' } });
    fireEvent.click(targccFilter);

    // Should only show Customer (matches search AND has TargCC)
    expect(screen.getByText('Customer')).toBeInTheDocument();
    expect(screen.queryByText('Product')).not.toBeInTheDocument();
    expect(screen.queryByText('Order')).not.toBeInTheDocument();
  });

  it('should toggle filter state on multiple clicks', () => {
    render(<SchemaViewer schema={mockSchema} />);
    const targccFilter = screen.getByText('TargCC Only');

    // First click - enable filter
    fireEvent.click(targccFilter);
    expect(screen.queryByText('Order')).not.toBeInTheDocument();

    // Second click - disable filter
    fireEvent.click(targccFilter);
    expect(screen.getByText('Order')).toBeInTheDocument();
  });

  it('should handle empty schema', () => {
    const emptySchema: DatabaseSchema = {
      tables: [],
      relationships: [],
    };

    render(<SchemaViewer schema={emptySchema} />);
    expect(screen.getByText('0 tables')).toBeInTheDocument();
  });
});
