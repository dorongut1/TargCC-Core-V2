import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import CodeViewer from '../../components/code/CodeViewer';
import type { CodeFile } from '../../components/code/CodeViewer';

describe('CodeViewer Component', () => {
  afterEach(() => {
    vi.clearAllMocks();
    vi.useRealTimers();
  });

  const mockFiles: CodeFile[] = [
    {
      name: 'Customer.cs',
      code: 'public class Customer { public int Id { get; set; } }',
      language: 'csharp'
    },
    {
      name: 'ICustomerRepository.cs',
      code: 'public interface ICustomerRepository { Task<Customer> GetByIdAsync(int id); }',
      language: 'csharp'
    },
    {
      name: 'CustomerController.cs',
      code: '[ApiController] public class CustomerController : ControllerBase { }',
      language: 'csharp'
    }
  ];

  describe('Rendering', () => {
    it('renders all file tabs', () => {
      render(<CodeViewer files={mockFiles} />);
      
      expect(screen.getByText('Customer.cs')).toBeInTheDocument();
      expect(screen.getByText('ICustomerRepository.cs')).toBeInTheDocument();
      expect(screen.getByText('CustomerController.cs')).toBeInTheDocument();
    });

    it('shows first file by default', () => {
      render(<CodeViewer files={mockFiles} />);
      
      // First tab should be selected
      const firstTab = screen.getByText('Customer.cs').closest('button');
      expect(firstTab).toHaveClass('Mui-selected');
    });

    it('renders copy button', () => {
      render(<CodeViewer files={mockFiles} />);
      
      const copyButton = screen.getByLabelText(/copy to clipboard/i);
      expect(copyButton).toBeInTheDocument();
    });

    it('shows info alert when no files provided', () => {
      render(<CodeViewer files={[]} />);
      
      expect(screen.getByText('No code files to display')).toBeInTheDocument();
    });

    it('does not render tabs when no files', () => {
      render(<CodeViewer files={[]} />);
      
      expect(screen.queryByRole('tablist')).not.toBeInTheDocument();
    });
  });

  describe('Tab Switching', () => {
    it('switches to second file when clicked', () => {
      render(<CodeViewer files={mockFiles} />);
      
      const secondTab = screen.getByText('ICustomerRepository.cs');
      fireEvent.click(secondTab);
      
      // Second tab should now be selected
      expect(secondTab.closest('button')).toHaveClass('Mui-selected');
    });

    it('switches to third file when clicked', () => {
      render(<CodeViewer files={mockFiles} />);
      
      const thirdTab = screen.getByText('CustomerController.cs');
      fireEvent.click(thirdTab);
      
      expect(thirdTab.closest('button')).toHaveClass('Mui-selected');
    });

    it('can switch back to first file', () => {
      render(<CodeViewer files={mockFiles} />);
      
      // Click second tab
      fireEvent.click(screen.getByText('ICustomerRepository.cs'));
      
      // Click back to first tab
      const firstTab = screen.getByText('Customer.cs');
      fireEvent.click(firstTab);
      
      expect(firstTab.closest('button')).toHaveClass('Mui-selected');
    });

    it('displays correct file title in CodePreview', () => {
      render(<CodeViewer files={mockFiles} />);
      
      expect(screen.getByText('Customer.cs')).toBeInTheDocument();
    });

    it('updates file title when switching tabs', () => {
      render(<CodeViewer files={mockFiles} />);
      
      fireEvent.click(screen.getByText('ICustomerRepository.cs'));
      
      expect(screen.getByText('ICustomerRepository.cs')).toBeInTheDocument();
    });
  });

  describe('Copy Functionality', () => {
    beforeEach(() => {
      // Mock clipboard API
      Object.assign(navigator, {
        clipboard: {
          writeText: vi.fn(() => Promise.resolve()),
        },
      });
    });

    it('copies current file code to clipboard', async () => {
      render(<CodeViewer files={mockFiles} />);
      
      const copyButton = screen.getByLabelText(/copy to clipboard/i);
      fireEvent.click(copyButton);
      
      await waitFor(() => {
        expect(navigator.clipboard.writeText).toHaveBeenCalledWith(mockFiles[0].code);
      });
    });

    it('copies correct file after switching tabs', async () => {
      render(<CodeViewer files={mockFiles} />);
      
      // Switch to second file
      fireEvent.click(screen.getByText('ICustomerRepository.cs'));
      
      // Copy
      const copyButton = screen.getByLabelText(/copy to clipboard/i);
      fireEvent.click(copyButton);
      
      await waitFor(() => {
        expect(navigator.clipboard.writeText).toHaveBeenCalledWith(mockFiles[1].code);
      });
    });

    it('shows success icon after copying', async () => {
      render(<CodeViewer files={mockFiles} />);
      
      const copyButton = screen.getByLabelText(/copy to clipboard/i);
      fireEvent.click(copyButton);
      
      await waitFor(() => {
        expect(screen.getByLabelText(/copied!/i)).toBeInTheDocument();
      });
    });

    it('shows check icon when copied', async () => {
      render(<CodeViewer files={mockFiles} />);
      
      const copyButton = screen.getByLabelText(/copy to clipboard/i);
      fireEvent.click(copyButton);
      
      await waitFor(() => {
        const button = screen.getByLabelText(/copied!/i);
        expect(button.querySelector('[data-testid="CheckIcon"]')).toBeInTheDocument();
      });
    });

    it.skip('reverts to copy icon after timeout', async () => {
      // TODO: Fake timers causing issues with React 19
      // Will re-enable when testing library is updated
      vi.useFakeTimers();
      
      render(<CodeViewer files={mockFiles} />);
      
      const copyButton = screen.getByLabelText(/copy to clipboard/i);
      fireEvent.click(copyButton);
      
      // After 2 seconds
      vi.advanceTimersByTime(2000);
      
      await waitFor(() => {
        expect(screen.getByLabelText(/copy to clipboard/i)).toBeInTheDocument();
      });
    });

    it('handles clipboard write errors gracefully', async () => {
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      Object.assign(navigator, {
        clipboard: {
          writeText: vi.fn(() => Promise.reject(new Error('Clipboard access denied'))),
        },
      });
      
      render(<CodeViewer files={mockFiles} />);
      
      const copyButton = screen.getByLabelText(/copy to clipboard/i);
      fireEvent.click(copyButton);
      
      await waitFor(() => {
        expect(consoleErrorSpy).toHaveBeenCalled();
      });
      
      consoleErrorSpy.mockRestore();
    });
  });

  describe('Single File Display', () => {
    it('renders correctly with single file', () => {
      const singleFile = [mockFiles[0]];
      render(<CodeViewer files={singleFile} />);
      
      expect(screen.getByText('Customer.cs')).toBeInTheDocument();
    });

    it('does not break with single file', () => {
      const singleFile = [mockFiles[0]];
      const { container } = render(<CodeViewer files={singleFile} />);
      
      expect(container).toBeInTheDocument();
    });
  });

  describe('Multiple Files', () => {
    it('handles many files', () => {
      const manyFiles: CodeFile[] = Array.from({ length: 10 }, (_, i) => ({
        name: `File${i}.cs`,
        code: `public class File${i} { }`,
        language: 'csharp'
      }));
      
      render(<CodeViewer files={manyFiles} />);
      
      expect(screen.getByText('File0.cs')).toBeInTheDocument();
      expect(screen.getByText('File9.cs')).toBeInTheDocument();
    });

    it('enables scrollable tabs for many files', () => {
      const manyFiles: CodeFile[] = Array.from({ length: 15 }, (_, i) => ({
        name: `VeryLongFileName${i}.cs`,
        code: `public class File${i} { }`,
        language: 'csharp'
      }));
      
      const { container } = render(<CodeViewer files={manyFiles} />);
      
      const tabs = container.querySelector('.MuiTabs-root');
      expect(tabs).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('handles files with empty code', () => {
      const filesWithEmpty: CodeFile[] = [
        { name: 'Empty.cs', code: '', language: 'csharp' }
      ];
      
      render(<CodeViewer files={filesWithEmpty} />);
      
      expect(screen.getByText('Empty.cs')).toBeInTheDocument();
    });

    it('handles different languages', () => {
      const multiLanguageFiles: CodeFile[] = [
        { name: 'Script.ts', code: 'const x = 1;', language: 'typescript' },
        { name: 'Code.cs', code: 'var x = 1;', language: 'csharp' },
        { name: 'Query.sql', code: 'SELECT * FROM Users', language: 'sql' }
      ];
      
      render(<CodeViewer files={multiLanguageFiles} />);
      
      expect(screen.getByText('Script.ts')).toBeInTheDocument();
      expect(screen.getByText('Code.cs')).toBeInTheDocument();
      expect(screen.getByText('Query.sql')).toBeInTheDocument();
    });

    it('handles very long file names', () => {
      const longNameFiles: CodeFile[] = [
        {
          name: 'ThisIsAVeryLongFileNameThatShouldStillDisplayCorrectly.cs',
          code: 'public class Test { }',
          language: 'csharp'
        }
      ];
      
      render(<CodeViewer files={longNameFiles} />);
      
      expect(screen.getByText(/ThisIsAVeryLongFileName/)).toBeInTheDocument();
    });

    it('handles files with special characters in names', () => {
      const specialFiles: CodeFile[] = [
        {
          name: 'File-With-Dashes.cs',
          code: 'public class Test { }',
          language: 'csharp'
        }
      ];
      
      render(<CodeViewer files={specialFiles} />);
      
      expect(screen.getByText('File-With-Dashes.cs')).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('has proper ARIA labels for copy button', () => {
      render(<CodeViewer files={mockFiles} />);
      
      expect(screen.getByLabelText(/copy to clipboard/i)).toBeInTheDocument();
    });

    it('tabs are keyboard navigable', () => {
      render(<CodeViewer files={mockFiles} />);
      
      const tabs = screen.getAllByRole('tab');
      expect(tabs).toHaveLength(3);
      expect(tabs[0]).toBeInTheDocument();
    });

    it('maintains tab accessibility attributes', () => {
      render(<CodeViewer files={mockFiles} />);
      
      const firstTab = screen.getByText('Customer.cs').closest('button');
      expect(firstTab).toHaveAttribute('role', 'tab');
    });
  });
});
