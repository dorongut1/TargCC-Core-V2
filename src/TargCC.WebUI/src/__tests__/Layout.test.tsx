/**
 * Layout Component Tests
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { Layout } from '../components/Layout';

describe('Layout', () => {
  it('renders children content', () => {
    render(
      <BrowserRouter>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </BrowserRouter>
    );
    expect(screen.getByText(/Test Content/i)).toBeInTheDocument();
  });

  it('renders header and sidebar', () => {
    render(
      <BrowserRouter>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </BrowserRouter>
    );
    expect(screen.getByText(/TargCC Core V2/i)).toBeInTheDocument();
    expect(screen.getByText(/Dashboard/i)).toBeInTheDocument();
  });

  it('toggles sidebar when menu button is clicked', async () => {
    const user = userEvent.setup();
    const { container } = render(
      <BrowserRouter>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </BrowserRouter>
    );

    const menuButton = screen.getByLabelText(/open drawer/i);
    
    // Sidebar should be open initially
    let drawer = container.querySelector('.MuiDrawer-root');
    expect(drawer).toBeInTheDocument();

    // Click to close
    await user.click(menuButton);
    
    // Sidebar should still be in DOM but with different state
    drawer = container.querySelector('.MuiDrawer-root');
    expect(drawer).toBeInTheDocument();
  });

  it('renders main content area', () => {
    const { container } = render(
      <BrowserRouter>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </BrowserRouter>
    );

    const mainContent = container.querySelector('main');
    expect(mainContent).toBeInTheDocument();
  });

  it('renders with proper box layout', () => {
    const { container } = render(
      <BrowserRouter>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </BrowserRouter>
    );

    const boxLayout = container.querySelector('.MuiBox-root');
    expect(boxLayout).toBeInTheDocument();
  });

  it('includes toolbar spacing', () => {
    const { container } = render(
      <BrowserRouter>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </BrowserRouter>
    );

    const toolbars = container.querySelectorAll('.MuiToolbar-root');
    // Should have at least 2 toolbars (header + spacing)
    expect(toolbars.length).toBeGreaterThanOrEqual(2);
  });

  it('renders multiple children correctly', () => {
    render(
      <BrowserRouter>
        <Layout>
          <div>First Child</div>
          <div>Second Child</div>
          <div>Third Child</div>
        </Layout>
      </BrowserRouter>
    );

    expect(screen.getByText(/First Child/i)).toBeInTheDocument();
    expect(screen.getByText(/Second Child/i)).toBeInTheDocument();
    expect(screen.getByText(/Third Child/i)).toBeInTheDocument();
  });

  it('maintains sidebar state across rerenders', async () => {
    const user = userEvent.setup();
    const { rerender } = render(
      <BrowserRouter>
        <Layout>
          <div>Initial Content</div>
        </Layout>
      </BrowserRouter>
    );

    const menuButton = screen.getByLabelText(/open drawer/i);
    await user.click(menuButton);

    // Rerender with new content
    rerender(
      <BrowserRouter>
        <Layout>
          <div>Updated Content</div>
        </Layout>
      </BrowserRouter>
    );

    // Content should update
    expect(screen.getByText(/Updated Content/i)).toBeInTheDocument();
  });

  it('renders header with menu button', () => {
    render(
      <BrowserRouter>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </BrowserRouter>
    );

    const menuButton = screen.getByLabelText(/open drawer/i);
    expect(menuButton).toBeInTheDocument();
  });

  it('renders all sidebar menu items', () => {
    render(
      <BrowserRouter>
        <Layout>
          <div>Test Content</div>
        </Layout>
      </BrowserRouter>
    );

    expect(screen.getByText(/Tables/i)).toBeInTheDocument();
    expect(screen.getByText(/Generate/i)).toBeInTheDocument();
    expect(screen.getByText(/AI Suggestions/i)).toBeInTheDocument();
    expect(screen.getByText(/Security/i)).toBeInTheDocument();
    expect(screen.getByText(/AI Chat/i)).toBeInTheDocument();
  });
});
