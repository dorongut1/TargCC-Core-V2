import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import TableCard from '../../components/schema/TableCard';
import { Table } from '../../types/schema';

describe('TableCard', () => {
  const mockTable: Table = {
    name: 'Customer',
    schema: 'dbo',
    rowCount: 1250,
    hasTargCCColumns: true,
    columns: [
      {
        name: 'CustomerId',
        type: 'int',
        nullable: false,
        isPrimaryKey: true,
        isForeignKey: false
      },
      {
        name: 'eno_Email',
        type: 'nvarchar',
        nullable: true,
        isPrimaryKey: false,
        isForeignKey: false,
        maxLength: 100
      }
    ]
  };

  it('renders table name with schema', () => {
    render(<TableCard table={mockTable} />);
    expect(screen.getByText(/dbo\.Customer/i)).toBeInTheDocument();
  });

  it('shows TargCC badge when table has TargCC columns', () => {
    render(<TableCard table={mockTable} />);
    expect(screen.getByText('TargCC')).toBeInTheDocument();
  });

  it('does not show TargCC badge when table has no TargCC columns', () => {
    const tableWithoutTargCC = { ...mockTable, hasTargCCColumns: false };
    render(<TableCard table={tableWithoutTargCC} />);
    expect(screen.queryByText('TargCC')).not.toBeInTheDocument();
  });

  it('displays column count', () => {
    render(<TableCard table={mockTable} />);
    expect(screen.getByText('2 columns')).toBeInTheDocument();
  });

  it('displays row count when available', () => {
    render(<TableCard table={mockTable} />);
    expect(screen.getByText('1,250 rows')).toBeInTheDocument();
  });

  it('handles singular column count', () => {
    const singleColumnTable = {
      ...mockTable,
      columns: [mockTable.columns[0]]
    };
    render(<TableCard table={singleColumnTable} />);
    expect(screen.getByText('1 column')).toBeInTheDocument();
  });

  it.skip('expands by default when defaultExpanded is true', () => {
    render(<TableCard table={mockTable} defaultExpanded={true} />);
    expect(screen.getByText('CustomerId')).toBeInTheDocument();
  });

  it.skip('collapses when defaultExpanded is false', () => {
    render(<TableCard table={mockTable} defaultExpanded={false} />);
    expect(screen.queryByText('CustomerId')).not.toBeInTheDocument();
  });

  it.skip('toggles expansion when expand button clicked', async () => {
    const user = userEvent.setup();
    render(<TableCard table={mockTable} defaultExpanded={true} />);
    
    const expandButton = screen.getByLabelText('show more');
    await user.click(expandButton);
    
    // After collapse, columns should not be visible
    expect(screen.queryByText('CustomerId')).not.toBeInTheDocument();
  });
});
