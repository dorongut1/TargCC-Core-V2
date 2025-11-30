import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import SchemaStats from '../../components/schema/SchemaStats';
import type { DatabaseSchema } from '../../types/schema';

/**
 * Test suite for SchemaStats component
 */
describe.skip('SchemaStats', () => {
  // Mock schema for testing
  const mockSchema: DatabaseSchema = {
    tables: [
      {
        name: 'Customer',
        schema: 'dbo',
        rowCount: 100,
        hasTargCCColumns: true,
        columns: [
          { name: 'Id', type: 'int', nullable: false, isPrimaryKey: true, isForeignKey: false },
          { name: 'Name', type: 'nvarchar', nullable: false, isPrimaryKey: false, isForeignKey: false },
          { name: 'Email', type: 'nvarchar', nullable: true, isPrimaryKey: false, isForeignKey: false },
        ],
      },
      {
        name: 'Order',
        schema: 'dbo',
        rowCount: 500,
        hasTargCCColumns: false,
        columns: [
          { name: 'Id', type: 'int', nullable: false, isPrimaryKey: true, isForeignKey: false },
          { name: 'Total', type: 'decimal', nullable: false, isPrimaryKey: false, isForeignKey: false },
        ],
      },
    ],
    relationships: [
      {
        fromTable: 'Order',
        fromColumn: 'CustomerId',
        toTable: 'Customer',
        toColumn: 'Id',
        type: 'one-to-many',
      },
    ],
  };

  it('should render schema statistics title', () => {
    render(<SchemaStats schema={mockSchema} />);
    expect(screen.getByText('Schema Statistics')).toBeInTheDocument();
  });

  it('should display total table count', () => {
    render(<SchemaStats schema={mockSchema} />);
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('Tables')).toBeInTheDocument();
  });

  it('should display total column count', () => {
    render(<SchemaStats schema={mockSchema} />);
    expect(screen.getByText('5')).toBeInTheDocument();
    expect(screen.getByText('Columns')).toBeInTheDocument();
  });

  it('should display relationship count', () => {
    render(<SchemaStats schema={mockSchema} />);
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('Relationships')).toBeInTheDocument();
  });

  it('should display TargCC table count', () => {
    render(<SchemaStats schema={mockSchema} />);
    expect(screen.getByText('TargCC Tables')).toBeInTheDocument();
    expect(screen.getByText('50% of total')).toBeInTheDocument();
  });

  it('should show average columns per table', () => {
    render(<SchemaStats schema={mockSchema} />);
    expect(screen.getByText(/Avg: 2.5 per table/)).toBeInTheDocument();
  });

  it('should display top data types section', () => {
    render(<SchemaStats schema={mockSchema} />);
    expect(screen.getByText('Top Data Types')).toBeInTheDocument();
  });

  it('should show data type distribution', () => {
    render(<SchemaStats schema={mockSchema} />);
    // nvarchar appears 2 times (40%)
    expect(screen.getByText('nvarchar')).toBeInTheDocument();
    expect(screen.getByText(/2.*40\.0%/)).toBeInTheDocument();
  });

  it('should handle empty schema', () => {
    const emptySchema: DatabaseSchema = {
      tables: [],
      relationships: [],
    };
    
    render(<SchemaStats schema={emptySchema} />);
    expect(screen.getByText('0')).toBeInTheDocument();
  });

  it('should calculate TargCC percentage correctly', () => {
    const allTargCCSchema: DatabaseSchema = {
      tables: [
        {
          name: 'Table1',
          schema: 'dbo',
          hasTargCCColumns: true,
          columns: [{ name: 'Id', type: 'int', nullable: false, isPrimaryKey: true, isForeignKey: false }],
        },
        {
          name: 'Table2',
          schema: 'dbo',
          hasTargCCColumns: true,
          columns: [{ name: 'Id', type: 'int', nullable: false, isPrimaryKey: true, isForeignKey: false }],
        },
      ],
      relationships: [],
    };
    
    render(<SchemaStats schema={allTargCCSchema} />);
    expect(screen.getByText('100% of total')).toBeInTheDocument();
  });
});
