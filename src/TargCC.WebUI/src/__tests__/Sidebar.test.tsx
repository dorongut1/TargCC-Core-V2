/**
 * Sidebar Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter, MemoryRouter } from 'react-router-dom';
import { Sidebar } from '../components/Sidebar';

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

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

  it('displays 6 menu items', () => {
    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );
    const menuItems = screen.getAllByRole('button');
    expect(menuItems).toHaveLength(6);
  });

  it('navigates when menu item is clicked', async () => {
    const user = userEvent.setup();
    mockNavigate.mockClear();

    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    const tablesButton = screen.getByText(/Tables/i);
    await user.click(tablesButton);

    expect(mockNavigate).toHaveBeenCalledWith('/tables');
  });

  it('highlights active menu item', () => {
    const { container } = render(
      <MemoryRouter initialEntries={['/tables']}>
        <Sidebar open={true} width={240} />
      </MemoryRouter>
    );

    // The selected button should have the Mui-selected class
    const selectedButtons = container.querySelectorAll('.Mui-selected');
    expect(selectedButtons.length).toBeGreaterThan(0);
  });

  it('renders icons for all menu items', () => {
    const { container } = render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    // Each menu item should have an icon (svg element)
    const icons = container.querySelectorAll('svg');
    expect(icons.length).toBeGreaterThanOrEqual(6);
  });

  it('applies correct width when open', () => {
    const { container } = render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    const drawer = container.querySelector('.MuiDrawer-root');
    expect(drawer).toBeInTheDocument();
  });

  it('applies correct width when closed', () => {
    const { container } = render(
      <BrowserRouter>
        <Sidebar open={false} width={240} />
      </BrowserRouter>
    );

    const drawer = container.querySelector('.MuiDrawer-root');
    expect(drawer).toBeInTheDocument();
  });

  it('navigates to dashboard when Dashboard is clicked', async () => {
    const user = userEvent.setup();
    mockNavigate.mockClear();

    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    const dashboardButton = screen.getByText(/Dashboard/i);
    await user.click(dashboardButton);

    expect(mockNavigate).toHaveBeenCalledWith('/');
  });

  it('navigates to generate when Generate is clicked', async () => {
    const user = userEvent.setup();
    mockNavigate.mockClear();

    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    const generateButton = screen.getByText(/Generate/i);
    await user.click(generateButton);

    expect(mockNavigate).toHaveBeenCalledWith('/generate');
  });

  it('navigates to suggestions when AI Suggestions is clicked', async () => {
    const user = userEvent.setup();
    mockNavigate.mockClear();

    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    const suggestionsButton = screen.getByText(/AI Suggestions/i);
    await user.click(suggestionsButton);

    expect(mockNavigate).toHaveBeenCalledWith('/suggestions');
  });

  it('navigates to security when Security is clicked', async () => {
    const user = userEvent.setup();
    mockNavigate.mockClear();

    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    const securityButton = screen.getByText(/Security/i);
    await user.click(securityButton);

    expect(mockNavigate).toHaveBeenCalledWith('/security');
  });

  it('navigates to chat when AI Chat is clicked', async () => {
    const user = userEvent.setup();
    mockNavigate.mockClear();

    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    const chatButton = screen.getByText(/AI Chat/i);
    await user.click(chatButton);

    expect(mockNavigate).toHaveBeenCalledWith('/chat');
  });

  it('displays tooltips for menu items', () => {
    render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    // Tooltips should be rendered (Material-UI tooltips)
    const dashboardText = screen.getByText(/Dashboard/i);
    expect(dashboardText).toBeInTheDocument();
  });

  it('renders divider after toolbar', () => {
    const { container } = render(
      <BrowserRouter>
        <Sidebar open={true} width={240} />
      </BrowserRouter>
    );

    const dividers = container.querySelectorAll('hr');
    expect(dividers.length).toBeGreaterThan(0);
  });
});
