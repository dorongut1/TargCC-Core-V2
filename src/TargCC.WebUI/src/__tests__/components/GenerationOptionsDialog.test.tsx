import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import GenerationOptionsDialog from '../../components/generation/GenerationOptionsDialog';

describe('GenerationOptionsDialog', () => {
  const mockOnClose = vi.fn();
  const mockOnGenerate = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('should render dialog when open', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    expect(screen.getByText(/Generate Code for Users/i)).toBeInTheDocument();
  });

  it('should not render dialog when closed', () => {
    render(
      <GenerationOptionsDialog
        open={false}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    expect(screen.queryByText(/Generate Code for Users/i)).not.toBeInTheDocument();
  });

  it('should show bulk generation title for multiple tables', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users', 'Products']}
        isBulk={true}
      />
    );

    expect(screen.getByText(/Generate Code for 2 Tables/i)).toBeInTheDocument();
  });

  it('should have all checkboxes checked by default', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    const entityCheckbox = screen.getByRole('checkbox', { name: /Entity Class/i });
    const repoCheckbox = screen.getByRole('checkbox', { name: /Repository Interface/i });
    const serviceCheckbox = screen.getByRole('checkbox', { name: /Service Interface/i });
    const controllerCheckbox = screen.getByRole('checkbox', { name: /API Controller/i });
    const testsCheckbox = screen.getByRole('checkbox', { name: /Unit Tests/i });

    expect(entityCheckbox).toBeChecked();
    expect(repoCheckbox).toBeChecked();
    expect(serviceCheckbox).toBeChecked();
    expect(controllerCheckbox).toBeChecked();
    expect(testsCheckbox).toBeChecked();
  });

  it('should toggle checkbox when clicked', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    const entityCheckbox = screen.getByRole('checkbox', { name: /Entity Class/i });
    expect(entityCheckbox).toBeChecked();

    fireEvent.click(entityCheckbox);
    expect(entityCheckbox).not.toBeChecked();

    fireEvent.click(entityCheckbox);
    expect(entityCheckbox).toBeChecked();
  });

  it('should show warning when overwrite is enabled', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    expect(screen.queryByText(/Warning: Existing files will be overwritten/i)).not.toBeInTheDocument();

    const overwriteCheckbox = screen.getByRole('checkbox', { name: /Overwrite Existing Files/i });
    fireEvent.click(overwriteCheckbox);

    expect(screen.getByText(/Warning: Existing files will be overwritten/i)).toBeInTheDocument();
  });

  it('should disable generate button when nothing is selected', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    // Uncheck all
    fireEvent.click(screen.getByRole('checkbox', { name: /Entity Class/i }));
    fireEvent.click(screen.getByRole('checkbox', { name: /Repository Interface/i }));
    fireEvent.click(screen.getByRole('checkbox', { name: /Service Interface/i }));
    fireEvent.click(screen.getByRole('checkbox', { name: /API Controller/i }));
    fireEvent.click(screen.getByRole('checkbox', { name: /Unit Tests/i }));

    const generateButton = screen.getByRole('button', { name: /^Generate$/i });
    expect(generateButton).toBeDisabled();
    expect(screen.getByText(/Please select at least one item to generate/i)).toBeInTheDocument();
  });

  it('should call onGenerate with selected options when generate clicked', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    const generateButton = screen.getByRole('button', { name: /^Generate$/i });
    fireEvent.click(generateButton);

    expect(mockOnGenerate).toHaveBeenCalledWith({
      generateEntity: true,
      generateRepository: true,
      generateService: true,
      generateController: true,
      generateTests: true,
      overwriteExisting: false,
    });
    expect(mockOnClose).toHaveBeenCalled();
  });

  it('should call onClose when cancel clicked', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    const cancelButton = screen.getByRole('button', { name: /Cancel/i });
    fireEvent.click(cancelButton);

    expect(mockOnClose).toHaveBeenCalled();
    expect(mockOnGenerate).not.toHaveBeenCalled();
  });

  it('should select all options when select all clicked', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    // Uncheck entity first
    fireEvent.click(screen.getByRole('checkbox', { name: /Entity Class/i }));
    expect(screen.getByRole('checkbox', { name: /Entity Class/i })).not.toBeChecked();

    // Click Select All (get all buttons and find the first one)
    const selectAllButtons = screen.getAllByRole('button', { name: /Select All/i });
    fireEvent.click(selectAllButtons[0]);

    // All should be checked
    expect(screen.getByRole('checkbox', { name: /Entity Class/i })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: /Repository Interface/i })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: /Service Interface/i })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: /API Controller/i })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: /Unit Tests/i })).toBeChecked();
  });

  it('should deselect all options when deselect all clicked', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users']}
      />
    );

    const deselectAllButtons = screen.getAllByRole('button', { name: /Deselect All/i });
    fireEvent.click(deselectAllButtons[0]);

    expect(screen.getByRole('checkbox', { name: /Entity Class/i })).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: /Repository Interface/i })).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: /Service Interface/i })).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: /API Controller/i })).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: /Unit Tests/i })).not.toBeChecked();
  });

  it('should show bulk generation info alert', () => {
    render(
      <GenerationOptionsDialog
        open={true}
        onClose={mockOnClose}
        onGenerate={mockOnGenerate}
        tableNames={['Users', 'Products', 'Orders']}
        isBulk={true}
      />
    );

    expect(screen.getByText(/Code will be generated for 3 tables/i)).toBeInTheDocument();
  });
});
