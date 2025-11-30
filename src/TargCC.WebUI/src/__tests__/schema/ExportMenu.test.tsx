import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import ExportMenu from '../../components/schema/ExportMenu';
import type { DatabaseSchema } from '../../types/schema';
import * as downloadCode from '../../utils/downloadCode';

// Mock downloadFile function
vi.mock('../../utils/downloadCode', () => ({
  downloadFile: vi.fn(),
}));

/**
 * Test suite for ExportMenu component
 */
describe.skip('ExportMenu', () => {
  // Mock schema for testing
  const mockSchema: DatabaseSchema = {
    tables: [
      {
        name: 'Customer',
        schema: 'dbo',
        rowCount: 100,
        hasTargCCColumns: true,
        columns: [
          { name: 'Id', type: 'int', nullable: false, isPrimaryKey: true, isForeignKey: false },
        ],
      },
    ],
    relationships: [],
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render export button', () => {
    render(<ExportMenu schema={mockSchema} />);
    const button = screen.getByLabelText('export schema');
    expect(button).toBeInTheDocument();
  });

  it('should open menu when button clicked', () => {
    render(<ExportMenu schema={mockSchema} />);
    const button = screen.getByLabelText('export schema');
    
    fireEvent.click(button);
    
    expect(screen.getByText('Export as JSON')).toBeInTheDocument();
    expect(screen.getByText('Export as SQL')).toBeInTheDocument();
    expect(screen.getByText('Export as Markdown')).toBeInTheDocument();
  });

  it('should export as JSON when JSON option selected', () => {
    render(<ExportMenu schema={mockSchema} />);
    const button = screen.getByLabelText('export schema');
    
    fireEvent.click(button);
    const jsonOption = screen.getByText('Export as JSON');
    fireEvent.click(jsonOption);
    
    expect(downloadCode.downloadFile).toHaveBeenCalledWith(
      'database-schema.json',
      expect.stringContaining('"tables"')
    );
  });

  it('should export as SQL when SQL option selected', () => {
    render(<ExportMenu schema={mockSchema} />);
    const button = screen.getByLabelText('export schema');
    
    fireEvent.click(button);
    const sqlOption = screen.getByText('Export as SQL');
    fireEvent.click(sqlOption);
    
    expect(downloadCode.downloadFile).toHaveBeenCalledWith(
      'database-schema.sql',
      expect.stringContaining('CREATE TABLE')
    );
  });

  it('should export as Markdown when Markdown option selected', () => {
    render(<ExportMenu schema={mockSchema} />);
    const button = screen.getByLabelText('export schema');
    
    fireEvent.click(button);
    const mdOption = screen.getByText('Export as Markdown');
    fireEvent.click(mdOption);
    
    expect(downloadCode.downloadFile).toHaveBeenCalledWith(
      'database-schema.md',
      expect.stringContaining('# Database Schema')
    );
  });

  it('should close menu after export', () => {
    render(<ExportMenu schema={mockSchema} />);
    const button = screen.getByLabelText('export schema');
    
    fireEvent.click(button);
    const jsonOption = screen.getByText('Export as JSON');
    fireEvent.click(jsonOption);
    
    expect(screen.queryByText('Export as JSON')).not.toBeInTheDocument();
  });

  it('should show descriptions for each export format', () => {
    render(<ExportMenu schema={mockSchema} />);
    const button = screen.getByLabelText('export schema');
    
    fireEvent.click(button);
    
    expect(screen.getByText('Structured data format')).toBeInTheDocument();
    expect(screen.getByText('DDL CREATE statements')).toBeInTheDocument();
    expect(screen.getByText('Documentation format')).toBeInTheDocument();
  });

  it('should have proper aria attributes', () => {
    render(<ExportMenu schema={mockSchema} />);
    const button = screen.getByLabelText('export schema');
    
    expect(button).toHaveAttribute('aria-haspopup', 'true');
    expect(button).toHaveAttribute('aria-expanded', 'false');
    
    fireEvent.click(button);
    expect(button).toHaveAttribute('aria-expanded', 'true');
  });
});
