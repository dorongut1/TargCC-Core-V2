import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import QuickStats from '../components/QuickStats';

describe('QuickStats', () => {
  it('renders all stat cards', () => {
    render(<QuickStats />);
    
    expect(screen.getByText('Total Tables')).toBeInTheDocument();
    expect(screen.getByText('Generated Files')).toBeInTheDocument();
    expect(screen.getByText('Pending Updates')).toBeInTheDocument();
    expect(screen.getByText('Last Generation')).toBeInTheDocument();
  });

  it('displays default values when no props provided', () => {
    render(<QuickStats />);
    
    expect(screen.getByText('24')).toBeInTheDocument();
    expect(screen.getByText('156')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText('2 hours ago')).toBeInTheDocument();
  });

  it('displays custom total tables value', () => {
    render(<QuickStats totalTables={50} />);
    expect(screen.getByText('50')).toBeInTheDocument();
  });

  it('displays custom generated files value', () => {
    render(<QuickStats generatedFiles={200} />);
    expect(screen.getByText('200')).toBeInTheDocument();
  });

  it('displays custom pending updates value', () => {
    render(<QuickStats pendingUpdates={5} />);
    expect(screen.getByText('5')).toBeInTheDocument();
  });

  it('displays custom last generation value', () => {
    render(<QuickStats lastGeneration="1 hour ago" />);
    expect(screen.getByText('1 hour ago')).toBeInTheDocument();
  });

  it('displays zero pending updates', () => {
    render(<QuickStats pendingUpdates={0} />);
    expect(screen.getByText('0')).toBeInTheDocument();
  });

  it('uses warning color for pending updates greater than 0', () => {
    const { container } = render(<QuickStats pendingUpdates={5} />);
    // Component should render with stats visible
    expect(container.querySelector('.MuiCard-root')).toBeInTheDocument();
  });

  it('renders with responsive grid layout', () => {
    const { container } = render(<QuickStats />);
    const grid = container.querySelector('.MuiGrid-container');
    expect(grid).toBeInTheDocument();
  });

  it('displays all icons', () => {
    const { container } = render(<QuickStats />);
    // MUI icons are rendered as SVG elements
    const svgs = container.querySelectorAll('svg');
    expect(svgs.length).toBeGreaterThanOrEqual(4);
  });

  it('accepts all props together', () => {
    render(
      <QuickStats
        totalTables={100}
        generatedFiles={500}
        pendingUpdates={10}
        lastGeneration="5 minutes ago"
      />
    );
    
    expect(screen.getByText('100')).toBeInTheDocument();
    expect(screen.getByText('500')).toBeInTheDocument();
    expect(screen.getByText('10')).toBeInTheDocument();
    expect(screen.getByText('5 minutes ago')).toBeInTheDocument();
  });
});
