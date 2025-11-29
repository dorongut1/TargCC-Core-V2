/**
 * Sidebar Component Tests
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { Sidebar } from '../components/Sidebar';

describe('Sidebar', () => {
  it('renders all menu items', () => {
    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );
    expect(screen.getByText(/Dashboard/i)).toBeInTheDocument();
    expect(screen.getByText(/Tables/i)).toBeInTheDocument();
    expect(screen.getByText(/Generate/i)).toBeInTheDocument();
    expect(screen.getByText(/AI Suggestions/i)).toBeInTheDocument();
    expect(screen.getByText(/Security/i)).toBeInTheDocument();
    expect(screen.getByText(/AI Chat/i)).toBeInTheDocument();
  });

  it('renders when closed', () => {
    render(
      <BrowserRouter>
        <Sidebar open={false} width={240} />
      </BrowserRouter>
    );
    // Sidebar should still render menu items even when closed
    expect(screen.getByText(/Dashboard/i)).toBeInTheDocument();
  });
});
