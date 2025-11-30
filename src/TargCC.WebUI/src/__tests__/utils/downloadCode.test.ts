import { describe, it, expect, vi, beforeEach } from 'vitest';
import { downloadFile, downloadAllAsZip, getFileExtension } from '../../utils/downloadCode';

// Mock document and URL
global.URL.createObjectURL = vi.fn(() => 'blob:mock-url');
global.URL.revokeObjectURL = vi.fn();

describe('downloadCode utilities', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    document.body.innerHTML = '';
  });

  describe('downloadFile', () => {
    it('creates a blob with correct content', () => {
      const content = 'test content';
      downloadFile('test.txt', content);

      expect(global.URL.createObjectURL).toHaveBeenCalled();
    });

    it('revokes the object URL', () => {
      downloadFile('test.txt', 'content');

      expect(global.URL.revokeObjectURL).toHaveBeenCalled();
    });

    it('cleans up after download', () => {
      downloadFile('test.txt', 'content');

      // Link should be removed from DOM
      const link = document.querySelector('a');
      expect(link).toBeNull();
    });
  });

  describe('getFileExtension', () => {
    it('returns .cs for csharp', () => {
      expect(getFileExtension('csharp')).toBe('.cs');
    });

    it('returns .ts for typescript', () => {
      expect(getFileExtension('typescript')).toBe('.ts');
    });

    it('returns .js for javascript', () => {
      expect(getFileExtension('javascript')).toBe('.js');
    });

    it('returns .sql for sql', () => {
      expect(getFileExtension('sql')).toBe('.sql');
    });

    it('returns .json for json', () => {
      expect(getFileExtension('json')).toBe('.json');
    });

    it('returns .txt for unknown language', () => {
      expect(getFileExtension('unknown')).toBe('.txt');
    });
  });

  describe('downloadAllAsZip', () => {
    it('creates a ZIP with all files', async () => {
      const files = [
        { name: 'file1.cs', code: 'content1' },
        { name: 'file2.cs', code: 'content2' },
      ];

      await downloadAllAsZip(files);

      expect(global.URL.createObjectURL).toHaveBeenCalled();
    });

    it('revokes the object URL after download', async () => {
      const files = [{ name: 'test.cs', code: 'content' }];
      
      await downloadAllAsZip(files);

      expect(global.URL.revokeObjectURL).toHaveBeenCalled();
    });

    it('cleans up the DOM after download', async () => {
      const files = [{ name: 'test.cs', code: 'content' }];
      
      await downloadAllAsZip(files);

      // Link should be removed
      const link = document.querySelector('a');
      expect(link).toBeNull();
    });

    it('handles multiple files correctly', async () => {
      const files = [
        { name: 'file1.cs', code: 'content1' },
        { name: 'file2.ts', code: 'content2' },
        { name: 'file3.sql', code: 'content3' },
      ];
      
      await downloadAllAsZip(files);

      expect(global.URL.createObjectURL).toHaveBeenCalled();
    });
  });
});
