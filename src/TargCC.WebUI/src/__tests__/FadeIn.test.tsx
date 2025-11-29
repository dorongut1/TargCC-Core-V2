import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import FadeIn from '../components/FadeIn';

describe('FadeIn', () => {
  it('renders without crashing', () => {
    render(
      <FadeIn>
        <div>Test content</div>
      </FadeIn>
    );
    
    expect(screen.getByText('Test content')).toBeInTheDocument();
  });

  it('renders children', () => {
    render(
      <FadeIn>
        <div data-testid="child">Child component</div>
      </FadeIn>
    );
    
    expect(screen.getByTestId('child')).toBeInTheDocument();
    expect(screen.getByText('Child component')).toBeInTheDocument();
  });

  it('wraps children in Box component', () => {
    const { container } = render(
      <FadeIn>
        <div>Test</div>
      </FadeIn>
    );
    
    expect(container.querySelector('.MuiBox-root')).toBeInTheDocument();
  });

  it('wraps children in Fade component', () => {
    const { container } = render(
      <FadeIn>
        <div>Test</div>
      </FadeIn>
    );
    
    // Fade component adds transition styles
    const fadeElement = container.querySelector('[style*="transition"]');
    expect(fadeElement).toBeInTheDocument();
  });

  it('accepts delay prop', () => {
    render(
      <FadeIn delay={100}>
        <div>Delayed content</div>
      </FadeIn>
    );
    
    expect(screen.getByText('Delayed content')).toBeInTheDocument();
  });

  it('accepts timeout prop', () => {
    render(
      <FadeIn timeout={1000}>
        <div>Slow fade</div>
      </FadeIn>
    );
    
    expect(screen.getByText('Slow fade')).toBeInTheDocument();
  });

  it('applies default delay of 0', () => {
    const { container } = render(
      <FadeIn>
        <div>No delay</div>
      </FadeIn>
    );
    
    const fadeElement = container.querySelector('[style*="transition"]');
    expect(fadeElement).toHaveStyle({ transitionDelay: '0ms' });
  });

  it('applies custom delay', () => {
    const { container } = render(
      <FadeIn delay={200}>
        <div>Delayed</div>
      </FadeIn>
    );
    
    const fadeElement = container.querySelector('[style*="transition"]');
    expect(fadeElement).toHaveStyle({ transitionDelay: '200ms' });
  });

  it('renders multiple children', () => {
    render(
      <FadeIn>
        <div>Child 1</div>
        <div>Child 2</div>
      </FadeIn>
    );
    
    expect(screen.getByText('Child 1')).toBeInTheDocument();
    expect(screen.getByText('Child 2')).toBeInTheDocument();
  });

  it('handles complex children', () => {
    render(
      <FadeIn>
        <div>
          <h1>Title</h1>
          <p>Paragraph</p>
          <button>Button</button>
        </div>
      </FadeIn>
    );
    
    expect(screen.getByRole('heading', { name: 'Title' })).toBeInTheDocument();
    expect(screen.getByText('Paragraph')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Button' })).toBeInTheDocument();
  });

  it('starts with "in" state true', () => {
    const { container } = render(
      <FadeIn>
        <div>Test</div>
      </FadeIn>
    );
    
    // Fade component with in=true should be visible
    const content = screen.getByText('Test');
    expect(content).toBeVisible();
  });
});
