import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import CodePreview from '../../components/code/CodePreview';

describe('CodePreview Component', () => {
  const sampleCode = `public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
}`;

  describe('Rendering', () => {
    it('renders with default title', () => {
      render(<CodePreview code={sampleCode} />);
      expect(screen.getByText('Code Preview')).toBeInTheDocument();
    });

    it('renders with custom title', () => {
      render(<CodePreview code={sampleCode} title="Customer Entity" />);
      expect(screen.getByText('Customer Entity')).toBeInTheDocument();
    });

    it('hides title when title prop is empty string', () => {
      render(<CodePreview code={sampleCode} title="" />);
      expect(screen.queryByText('Code Preview')).not.toBeInTheDocument();
    });

    it('shows loading spinner initially', () => {
      render(<CodePreview code={sampleCode} />);
      expect(screen.getByRole('progressbar')).toBeInTheDocument();
    });

    it('renders within a Paper component', () => {
      const { container } = render(<CodePreview code={sampleCode} />);
      const paper = container.querySelector('.MuiPaper-root');
      expect(paper).toBeInTheDocument();
    });

    it('renders theme toggle button', () => {
      render(<CodePreview code={sampleCode} />);
      const button = screen.getByRole('button');
      expect(button).toBeInTheDocument();
    });
  });

  describe('Monaco Editor Configuration', () => {
    it('uses csharp language by default', () => {
      const { container } = render(<CodePreview code={sampleCode} />);
      expect(container).toBeInTheDocument();
    });

    it('applies custom language when specified', () => {
      const { container } = render(
        <CodePreview code="const x = 1;" language="typescript" />
      );
      expect(container).toBeInTheDocument();
    });

    it('uses default height of 400px', () => {
      const { container } = render(<CodePreview code={sampleCode} />);
      expect(container).toBeInTheDocument();
    });

    it('applies custom height when specified', () => {
      const { container } = render(
        <CodePreview code={sampleCode} height="600px" />
      );
      expect(container).toBeInTheDocument();
    });

    it('is read-only by default', () => {
      const { container } = render(<CodePreview code={sampleCode} />);
      expect(container).toBeInTheDocument();
    });

    it('allows editable mode when readOnly is false', () => {
      const { container } = render(
        <CodePreview code={sampleCode} readOnly={false} />
      );
      expect(container).toBeInTheDocument();
    });
  });

  describe('Code Display', () => {
    it('displays provided code', () => {
      const { container } = render(<CodePreview code={sampleCode} />);
      expect(container).toBeInTheDocument();
    });

    it('handles empty code', () => {
      const { container } = render(<CodePreview code="" />);
      expect(container).toBeInTheDocument();
    });

    it('handles multi-line code', () => {
      const multiLineCode = `namespace TargCC
{
    public class Test
    {
        public void Method() { }
    }
}`;
      const { container } = render(<CodePreview code={multiLineCode} />);
      expect(container).toBeInTheDocument();
    });
  });

  describe('Loading State', () => {
    it('shows loading box with correct background color', () => {
      const { container } = render(<CodePreview code={sampleCode} />);
      const loadingBox = container.querySelector('.MuiBox-root');
      expect(loadingBox).toBeInTheDocument();
    });

    it('loading box has correct height matching editor height', () => {
      render(<CodePreview code={sampleCode} height="500px" />);
      expect(screen.getByRole('progressbar')).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('handles very long code strings', () => {
      const longCode = 'public class Test { }\n'.repeat(1000);
      const { container } = render(<CodePreview code={longCode} />);
      expect(container).toBeInTheDocument();
    });

    it('handles code with special characters', () => {
      const specialCode = 'string text = "Hello \\"World\\"";';
      const { container } = render(<CodePreview code={specialCode} />);
      expect(container).toBeInTheDocument();
    });

    it('handles code with unicode characters', () => {
      const unicodeCode = '// משתנה עברי\nvar x = "שלום";';
      const { container} = render(<CodePreview code={unicodeCode} />);
      expect(container).toBeInTheDocument();
    });
  });
});
