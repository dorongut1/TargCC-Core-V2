import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import DashboardSkeleton from '../components/DashboardSkeleton';

describe('DashboardSkeleton', () => {
  it('renders without crashing', () => {
    render(<DashboardSkeleton />);
    expect(document.querySelector('.MuiSkeleton-root')).toBeInTheDocument();
  });

  it('renders page title skeleton', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Title skeleton should be text variant
    const titleSkeleton = container.querySelector('.MuiSkeleton-text');
    expect(titleSkeleton).toBeInTheDocument();
  });

  it('renders 4 quick stat card skeletons', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Should have 4 stat cards in first grid
    const cards = container.querySelectorAll('.MuiCard-root');
    expect(cards.length).toBeGreaterThanOrEqual(4);
  });

  it('renders recent generations widget skeleton', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Should have circular skeletons for icons
    const circularSkeletons = container.querySelectorAll('.MuiSkeleton-circular');
    expect(circularSkeletons.length).toBeGreaterThan(0);
  });

  it('renders activity timeline widget skeleton', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Multiple Paper components for widgets
    const papers = container.querySelectorAll('.MuiPaper-root');
    expect(papers.length).toBeGreaterThan(0);
  });

  it('renders system health widget skeleton', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Should have rectangular skeletons for progress bars
    const rectangularSkeletons = container.querySelectorAll('.MuiSkeleton-rectangular');
    expect(rectangularSkeletons.length).toBeGreaterThan(0);
  });

  it('renders schema stats widget skeleton', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Check for multiple skeleton variants
    const allSkeletons = container.querySelectorAll('.MuiSkeleton-root');
    expect(allSkeletons.length).toBeGreaterThan(10);
  });

  it('matches dashboard grid layout', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Should have Grid containers
    const grids = container.querySelectorAll('.MuiGrid-root');
    expect(grids.length).toBeGreaterThan(0);
  });

  it('has proper spacing between elements', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Box component should be present
    const boxes = container.querySelectorAll('.MuiBox-root');
    expect(boxes.length).toBeGreaterThan(0);
  });

  it('uses correct skeleton variants', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Should use text skeletons
    expect(container.querySelector('.MuiSkeleton-text')).toBeInTheDocument();
    
    // Should use circular skeletons (for icons)
    expect(container.querySelector('.MuiSkeleton-circular')).toBeInTheDocument();
    
    // Should use rectangular skeletons (for progress bars)
    expect(container.querySelector('.MuiSkeleton-rectangular')).toBeInTheDocument();
  });

  it('renders skeleton for all dashboard sections', () => {
    const { container } = render(<DashboardSkeleton />);
    
    // Verify minimum number of skeleton elements
    const allSkeletons = container.querySelectorAll('.MuiSkeleton-root');
    
    // Should have:
    // - 1 title
    // - 8 stat card elements (4 cards × 2 elements each)
    // - Multiple widget elements
    expect(allSkeletons.length).toBeGreaterThanOrEqual(15);
  });
});
