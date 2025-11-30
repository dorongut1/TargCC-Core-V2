import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import ColumnList from '../../components/schema/ColumnList';
import { Column } from '../../types/schema';

describe.skip('ColumnList', () => {
  const mockColumns: Column[] = [
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
    },
    {
      name: 'OrderId',
      type: 'int',
      nullable: false,
      isPrimaryKey: false,
      isForeignKey: true,
      foreignKeyTable: 'Order',
      foreignKeyColumn: 'OrderId'
    }
  ];

  it('renders all columns', () => {
    render(<ColumnList columns={mockColumns} />);
    
    expect(screen.getByText('CustomerId')).toBeInTheDocument();
    expect(screen.getByText('eno_Email')).toBeInTheDocument();
    expect(screen.getByText('OrderId')).toBeInTheDocument();
  });

  it('displays column types correctly', () => {
    render(<ColumnList columns={mockColumns} />);
    
    expect(screen.getByText('int')).toBeInTheDocument();
    expect(screen.getByText('nvarchar(100)')).toBeInTheDocument();
  });

  it('shows primary key icon for primary keys', () => {
    const { container } = render(<ColumnList columns={mockColumns} />);
    const pkIcons = container.querySelectorAll('[data-testid="KeyIcon"]');
    expect(pkIcons.length).toBeGreaterThan(0);
  });

  it('shows foreign key icon for foreign keys', () => {
    const { container } = render(<ColumnList columns={mockColumns} />);
    const fkIcons = container.querySelectorAll('[data-testid="LinkIcon"]');
    expect(fkIcons.length).toBeGreaterThan(0);
  });

  it('displays NOT NULL badge for non-nullable columns', () => {
    render(<ColumnList columns={mockColumns} />);
    const notNullBadges = screen.getAllByText('NOT NULL');
    expect(notNullBadges.length).toBeGreaterThan(0);
  });

  it('does not show NOT NULL for nullable columns', () => {
    const nullableColumns: Column[] = [{
      name: 'OptionalField',
      type: 'nvarchar',
      nullable: true,
      isPrimaryKey: false,
      isForeignKey: false
    }];
    
    render(<ColumnList columns={nullableColumns} />);
    expect(screen.queryByText('NOT NULL')).not.toBeInTheDocument();
  });

  it('shows default value when present', () => {
    const columnsWithDefault: Column[] = [{
      name: 'Status',
      type: 'nvarchar',
      nullable: false,
      isPrimaryKey: false,
      isForeignKey: false,
      defaultValue: 'Active'
    }];
    
    render(<ColumnList columns={columnsWithDefault} />);
    expect(screen.getByText('= Active')).toBeInTheDocument();
  });
});
