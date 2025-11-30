import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import GenerationWizard from '../../components/wizard/GenerationWizard';

describe('GenerationWizard', () => {
  it('renders the wizard with title', () => {
    render(<GenerationWizard />);
    expect(screen.getByText('Code Generation Wizard')).toBeInTheDocument();
  });

  it('renders all 4 steps in stepper', () => {
    render(<GenerationWizard />);
    expect(screen.getByText('Select Tables')).toBeInTheDocument();
    expect(screen.getByText('Choose Options')).toBeInTheDocument();
    expect(screen.getByText('Review')).toBeInTheDocument();
    expect(screen.getByText('Generate')).toBeInTheDocument();
  });

  it('starts at step 0 (Select Tables)', () => {
    render(<GenerationWizard />);
    expect(screen.getByText('Select Tables to Generate')).toBeInTheDocument();
  });

  it('Next button is visible and enabled', () => {
    render(<GenerationWizard />);
    const nextButton = screen.getByRole('button', { name: /next/i });
    expect(nextButton).toBeInTheDocument();
    expect(nextButton).toBeEnabled();
  });

  it('Back button is disabled on first step', () => {
    render(<GenerationWizard />);
    const backButton = screen.getByRole('button', { name: /back/i });
    expect(backButton).toBeDisabled();
  });

  it('shows validation error when trying to advance without selecting tables', () => {
    render(<GenerationWizard />);
    const nextButton = screen.getByRole('button', { name: /next/i });
    fireEvent.click(nextButton);
    expect(screen.getByText('Please select at least one table')).toBeInTheDocument();
  });

  it('advances to step 2 when table is selected and Next is clicked', () => {
    render(<GenerationWizard />);

    // Select a table
    const customerCheckbox = screen.getByRole('checkbox', { name: /customer/i });
    fireEvent.click(customerCheckbox);

    // Click Next
    const nextButton = screen.getByRole('button', { name: /next/i });
    fireEvent.click(nextButton);

    // Should now be on step 2
    expect(screen.getByText('Choose What to Generate')).toBeInTheDocument();
  });

  it('Back button goes to previous step', () => {
    render(<GenerationWizard />);

    // Select a table and go to step 2
    const customerCheckbox = screen.getByRole('checkbox', { name: /customer/i });
    fireEvent.click(customerCheckbox);
    const nextButton = screen.getByRole('button', { name: /next/i });
    fireEvent.click(nextButton);

    // Now on step 2, click Back
    const backButton = screen.getByRole('button', { name: /back/i });
    fireEvent.click(backButton);

    // Should be back on step 1
    expect(screen.getByText('Select Tables to Generate')).toBeInTheDocument();
  });

  it('shows validation error when trying to advance without selecting options', () => {
    render(<GenerationWizard />);

    // Select a table and go to step 2
    const customerCheckbox = screen.getByRole('checkbox', { name: /customer/i });
    fireEvent.click(customerCheckbox);
    fireEvent.click(screen.getByRole('button', { name: /next/i }));

    // Uncheck all options
    const entityCheckbox = screen.getByRole('checkbox', { name: /entity classes/i });
    const repoCheckbox = screen.getByRole('checkbox', { name: /repositories/i });
    const handlersCheckbox = screen.getByRole('checkbox', { name: /cqrs handlers/i });
    const apiCheckbox = screen.getByRole('checkbox', { name: /api controllers/i });

    fireEvent.click(entityCheckbox);
    fireEvent.click(repoCheckbox);
    fireEvent.click(handlersCheckbox);
    fireEvent.click(apiCheckbox);

    // Try to advance
    fireEvent.click(screen.getByRole('button', { name: /next/i }));

    expect(screen.getByText('Please select at least one generation option')).toBeInTheDocument();
  });

  it('advances to Review step when options are selected', () => {
    render(<GenerationWizard />);

    // Step 1: Select table
    const customerCheckbox = screen.getByRole('checkbox', { name: /customer/i });
    fireEvent.click(customerCheckbox);
    fireEvent.click(screen.getByRole('button', { name: /next/i }));

    // Step 2: Options are already selected by default, just click Next
    fireEvent.click(screen.getByRole('button', { name: /next/i }));

    // Should now be on Review step
    expect(screen.getByText('Review Your Selection')).toBeInTheDocument();
  });

  it('shows Generate button on last step', () => {
    render(<GenerationWizard />);

    // Navigate to last step
    fireEvent.click(screen.getByRole('checkbox', { name: /customer/i }));
    fireEvent.click(screen.getByRole('button', { name: /next/i })); // Step 2
    fireEvent.click(screen.getByRole('button', { name: /next/i })); // Step 3 (Review)
    fireEvent.click(screen.getByRole('button', { name: /next/i })); // Step 4 (Generate)

    expect(screen.getByRole('button', { name: /generate/i })).toBeInTheDocument();
  });

  it('clears validation error when going back', () => {
    render(<GenerationWizard />);

    // Try to advance without selecting table
    fireEvent.click(screen.getByRole('button', { name: /next/i }));
    expect(screen.getByText('Please select at least one table')).toBeInTheDocument();

    // Go back (even though we're on step 1, this tests the error clearing logic)
    const customerCheckbox = screen.getByRole('checkbox', { name: /customer/i });
    fireEvent.click(customerCheckbox);
    fireEvent.click(screen.getByRole('button', { name: /next/i }));

    // Error should be cleared
    expect(screen.queryByText('Please select at least one table')).not.toBeInTheDocument();
  });
});
