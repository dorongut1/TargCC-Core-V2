import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import SchemaStats from '../components/SchemaStats';

describe('SchemaStats', () => {
  it('renders component with title', () => {
    render(<SchemaStats />);
    expect(screen.getByText('Schema Statistics')).toBeInTheDocument();
  });

  it('displays default average columns per table', () => {
    render(<SchemaStats />);
    expect(screen.getByText('8.5')).toBeInTheDocument();
    expect(screen.getByText('Avg Columns/Table')).toBeInTheDocument();
  });

  it('displays default relationship count', () => {
    render(<SchemaStats />);
    expect(screen.getByText('42')).toBeInTheDocument();
    expect(screen.getByText('Relationships')).toBeInTheDocument();
  });

  it('displays custom average columns per table', () => {
    render(<SchemaStats averageColumnsPerTable={12.3} />);
    expect(screen.getByText('12.3')).toBeInTheDocument();
  });

  it('displays custom relationship count', () => {
    render(<SchemaStats relationshipCount={100} />);
    expect(screen.getByText('100')).toBeInTheDocument();
  });

  it('displays schema statistics section', () => {
    render(<SchemaStats />);
    expect(screen.getByText('Tables by Schema')).toBeInTheDocument();
  });

  it('displays data type statistics section', () => {
    render(<SchemaStats />);
    expect(screen.getByText('Most Common Data Types')).toBeInTheDocument();
  });

  it('renders mock schemas when none provided', () => {
    render(<SchemaStats />);
    expect(screen.getByText('dbo')).toBeInTheDocument();
    expect(screen.getByText('Sales')).toBeInTheDocument();
  });

  it('renders mock data types when none provided', () => {
    render(<SchemaStats />);
    expect(screen.getByText('VARCHAR')).toBeInTheDocument();
    expect(screen.getByText('INT')).toBeInTheDocument();
  });

  it('displays custom schemas', () => {
    const schemas = [
      { schemaName: 'custom', tableCount: 10, percentage: 100 }
    ];
    
    render(<SchemaStats schemas={schemas} />);
    expect(screen.getByText('custom')).toBeInTheDocument();
    expect(screen.getByText(/10/)).toBeInTheDocument();
  });

  it('displays custom data types', () => {
    const dataTypes = [
      { type: 'BIGINT', count: 50, percentage: 100 }
    ];
    
    render(<SchemaStats dataTypes={dataTypes} />);
    expect(screen.getByText('BIGINT')).toBeInTheDocument();
    expect(screen.getByText(/50/)).toBeInTheDocument();
  });

  it('shows percentage for schemas', () => {
    render(<SchemaStats />);
    expect(screen.getByText(/62.5%/)).toBeInTheDocument(); // dbo schema
  });

  it('shows percentage for data types', () => {
    render(<SchemaStats />);
    expect(screen.getByText(/42.5%/)).toBeInTheDocument(); // VARCHAR
  });

  it('renders progress bars for schemas', () => {
    const { container } = render(<SchemaStats />);
    const progressBars = container.querySelectorAll('.MuiLinearProgress-root');
    expect(progressBars.length).toBeGreaterThan(0);
  });

  it('accepts all props together', () => {
    const schemas = [
      { schemaName: 'test', tableCount: 5, percentage: 100 }
    ];
    const dataTypes = [
      { type: 'TEXT', count: 25, percentage: 100 }
    ];
    
    render(
      <SchemaStats
        schemas={schemas}
        dataTypes={dataTypes}
        averageColumnsPerTable={10}
        relationshipCount={50}
      />
    );
    
    expect(screen.getByText('test')).toBeInTheDocument();
    expect(screen.getByText('TEXT')).toBeInTheDocument();
    expect(screen.getByText('10')).toBeInTheDocument();
    expect(screen.getByText('50')).toBeInTheDocument();
  });
});
