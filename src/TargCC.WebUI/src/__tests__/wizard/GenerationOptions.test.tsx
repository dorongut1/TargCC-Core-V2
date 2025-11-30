import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import GenerationOptions from '../../components/wizard/GenerationOptions';
import type { WizardData } from '../../components/wizard/GenerationWizard';

describe('GenerationOptions', () => {
  const mockData: WizardData = {
    selectedTables: ['Customer'],
    options: {
      entities: true,
      repositories: true,
      handlers: true,
      api: true
    }
  };

  const mockOnChange = vi.fn();

  it('renders the component with title', () => {
    render(<GenerationOptions data={mockData} onChange={mockOnChange} />);
    expect(screen.getByText('Choose What to Generate')).toBeInTheDocument();
  });

  it('renders all option checkboxes with labels', () => {
    render(<GenerationOptions data={mockData} onChange={mockOnChange} />);
    expect(screen.getByText('Entity Classes')).toBeInTheDocument();
    expect(screen.getByText('Repositories')).toBeInTheDocument();
    expect(screen.getByText('CQRS Handlers')).toBeInTheDocument();
    expect(screen.getByText('API Controllers')).toBeInTheDocument();
  });

  it('renders option descriptions', () => {
    render(<GenerationOptions data={mockData} onChange={mockOnChange} />);
    expect(screen.getByText(/generate domain model classes/i)).toBeInTheDocument();
    expect(screen.getByText(/generate data access layer/i)).toBeInTheDocument();
    expect(screen.getByText(/generate command and query handlers/i)).toBeInTheDocument();
    expect(screen.getByText(/generate rest api endpoints/i)).toBeInTheDocument();
  });

  it('shows all checkboxes as checked by default', () => {
    render(<GenerationOptions data={mockData} onChange={mockOnChange} />);

    const entityCheckbox = screen.getByRole('checkbox', { name: /entity classes/i });
    const repoCheckbox = screen.getByRole('checkbox', { name: /repositories/i });
    const handlersCheckbox = screen.getByRole('checkbox', { name: /cqrs handlers/i });
    const apiCheckbox = screen.getByRole('checkbox', { name: /api controllers/i });

    expect(entityCheckbox).toBeChecked();
    expect(repoCheckbox).toBeChecked();
    expect(handlersCheckbox).toBeChecked();
    expect(apiCheckbox).toBeChecked();
  });

  it('calls onChange when entities option is toggled off', () => {
    render(<GenerationOptions data={mockData} onChange={mockOnChange} />);

    const entityCheckbox = screen.getByRole('checkbox', { name: /entity classes/i });
    fireEvent.click(entityCheckbox);

    expect(mockOnChange).toHaveBeenCalledWith({
      ...mockData,
      options: {
        entities: false,
        repositories: true,
        handlers: true,
        api: true
      }
    });
  });

  it('calls onChange when repositories option is toggled off', () => {
    render(<GenerationOptions data={mockData} onChange={mockOnChange} />);

    const repoCheckbox = screen.getByRole('checkbox', { name: /repositories/i });
    fireEvent.click(repoCheckbox);

    expect(mockOnChange).toHaveBeenCalledWith({
      ...mockData,
      options: {
        entities: true,
        repositories: false,
        handlers: true,
        api: true
      }
    });
  });

  it('calls onChange when option is toggled on', () => {
    const dataWithOptionOff = {
      ...mockData,
      options: {
        entities: false,
        repositories: true,
        handlers: true,
        api: true
      }
    };

    render(<GenerationOptions data={dataWithOptionOff} onChange={mockOnChange} />);

    const entityCheckbox = screen.getByRole('checkbox', { name: /entity classes/i });
    fireEvent.click(entityCheckbox);

    expect(mockOnChange).toHaveBeenCalledWith({
      ...mockData,
      options: {
        entities: true,
        repositories: true,
        handlers: true,
        api: true
      }
    });
  });

  it('shows warning when no options are selected', () => {
    const dataWithNoOptions = {
      ...mockData,
      options: {
        entities: false,
        repositories: false,
        handlers: false,
        api: false
      }
    };

    render(<GenerationOptions data={dataWithNoOptions} onChange={mockOnChange} />);

    expect(screen.getByText('Please select at least one generation option')).toBeInTheDocument();
  });

  it('does not show warning when at least one option is selected', () => {
    render(<GenerationOptions data={mockData} onChange={mockOnChange} />);

    expect(screen.queryByText('Please select at least one generation option')).not.toBeInTheDocument();
  });

  it('displays correct selection count', () => {
    render(<GenerationOptions data={mockData} onChange={mockOnChange} />);

    expect(screen.getByText('4 option(s) selected')).toBeInTheDocument();
  });

  it('updates selection count when options are toggled', () => {
    const dataWithTwoOptions = {
      ...mockData,
      options: {
        entities: true,
        repositories: true,
        handlers: false,
        api: false
      }
    };

    render(<GenerationOptions data={dataWithTwoOptions} onChange={mockOnChange} />);

    expect(screen.getByText('2 option(s) selected')).toBeInTheDocument();
  });

  it('shows 0 count when no options selected', () => {
    const dataWithNoOptions = {
      ...mockData,
      options: {
        entities: false,
        repositories: false,
        handlers: false,
        api: false
      }
    };

    render(<GenerationOptions data={dataWithNoOptions} onChange={mockOnChange} />);

    expect(screen.getByText('0 option(s) selected')).toBeInTheDocument();
  });
});
