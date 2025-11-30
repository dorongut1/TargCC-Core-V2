import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import SchemaViewer from '../../components/schema/SchemaViewer';
import { DatabaseSchema } from '../../types/schema';

describe('SchemaViewer', () => {
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
            isForeignKey: false
          }
        ]
      },
      {
        name: 'Order',
        schema: 'dbo',
        rowCount: 500,
        hasTargCCColumns: true,
        columns: [
          {
            name: 'OrderId',
            type: 'int',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false
          }
        ]
      },
      {
        name: 'Product',
        schema: 'dbo',
        rowCount: 50,
        hasTargCCColumns: false,
        columns: [
          {
            name: 'ProductId',
            type: 'int',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false
          }
        ]
      }
    ],
    relationships: []
  };

  it('renders database schema title', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText('Database Schema')).toBeInTheDocument();
  });

  it('displays total table count', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText('3 tables')).toBeInTheDocument();
  });

  it('displays TargCC table count', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText('2 TargCC')).toBeInTheDocument();
  });

  it('renders all tables initially', () => {
    render(<SchemaViewer schema={mockSchema} />);
    expect(screen.getByText(/dbo\.Customer/i)).toBeInTheDocument();
    expect(screen.getByText(/dbo\.Order/i)).toBeInTheDocument();
    expect(screen.getByText(/dbo\.Product/i)).toBeInTheDocument();
  });

  it.skip('filters tables by name when searching', async () => {
    const user = userEvent.setup();
    render(<SchemaViewer schema={mockSchema} />);
    
    const searchBox = screen.getByPlaceholderText('Search tables and columns...');
    await user.type(searchBox, 'Customer');
    
    expect(screen.getByText(/dbo\.Customer/i)).toBeInTheDocument();
    expect(screen.queryByText(/dbo\.Order/i)).not.toBeInTheDocument();
    expect(screen.queryByText(/dbo\.Product/i)).not.toBeInTheDocument();
  });

  it.skip('filters tables by column name when searching', async () => {
    const user = userEvent.setup();
    render(<SchemaViewer schema={mockSchema} />);
    
    const searchBox = screen.getByPlaceholderText('Search tables and columns...');
    await user.type(searchBox, 'OrderId');
    
    expect(screen.getByText(/dbo\.Order/i)).toBeInTheDocument();
    expect(screen.queryByText(/dbo\.Customer/i)).not.toBeInTheDocument();
  });

  it.skip('shows no results message when search has no matches', async () => {
    const user = userEvent.setup();
    render(<SchemaViewer schema={mockSchema} />);
    
    const searchBox = screen.getByPlaceholderText('Search tables and columns...');
    await user.type(searchBox, 'NonExistent');
    
    expect(screen.getByText('No tables found')).toBeInTheDocument();
    expect(screen.getByText(/No tables match your search: "NonExistent"/i)).toBeInTheDocument();
  });

  it.skip('search is case-insensitive', async () => {
    const user = userEvent.setup();
    render(<SchemaViewer schema={mockSchema} />);
    
    const searchBox = screen.getByPlaceholderText('Search tables and columns...');
    await user.type(searchBox, 'customer');
    
    expect(screen.getByText(/dbo\.Customer/i)).toBeInTheDocument();
  });
});
