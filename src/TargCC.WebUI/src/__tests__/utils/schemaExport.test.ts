import { describe, it, expect } from 'vitest';
import { exportAsJSON, exportAsSQL, exportAsMarkdown } from '../../utils/schemaExport';
import type { DatabaseSchema } from '../../types/schema';

/**
 * Test suite for schema export utilities
 */
describe('schemaExport', () => {
  // Mock schema for testing
  const mockSchema: DatabaseSchema = {
    tables: [
      {
        name: 'Customer',
        schema: 'dbo',
        rowCount: 100,
        hasTargCCColumns: true,
        columns: [
          {
            name: 'CustomerId',
            type: 'int',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false,
          },
          {
            name: 'Name',
            type: 'nvarchar',
            nullable: false,
            isPrimaryKey: false,
            isForeignKey: false,
            maxLength: 100,
          },
        ],
      },
      {
        name: 'Order',
        schema: 'dbo',
        rowCount: 500,
        hasTargCCColumns: false,
        columns: [
          {
            name: 'OrderId',
            type: 'int',
            nullable: false,
            isPrimaryKey: true,
            isForeignKey: false,
          },
          {
            name: 'CustomerId',
            type: 'int',
            nullable: false,
            isPrimaryKey: false,
            isForeignKey: true,
            foreignKeyTable: 'Customer',
            foreignKeyColumn: 'CustomerId',
          },
        ],
      },
    ],
    relationships: [
      {
        fromTable: 'Order',
        fromColumn: 'CustomerId',
        toTable: 'Customer',
        toColumn: 'CustomerId',
        type: 'one-to-many',
      },
    ],
  };

  describe('exportAsJSON', () => {
    it('should export schema as formatted JSON', () => {
      const result = exportAsJSON(mockSchema);
      
      expect(result).toContain('"tables"');
      expect(result).toContain('"relationships"');
      expect(result).toContain('Customer');
      expect(result).toContain('Order');
    });

    it('should be valid parseable JSON', () => {
      const result = exportAsJSON(mockSchema);
      const parsed = JSON.parse(result);
      
      expect(parsed.tables).toHaveLength(2);
      expect(parsed.relationships).toHaveLength(1);
    });

    it('should preserve all table properties', () => {
      const result = exportAsJSON(mockSchema);
      const parsed = JSON.parse(result);
      
      expect(parsed.tables[0].name).toBe('Customer');
      expect(parsed.tables[0].hasTargCCColumns).toBe(true);
      expect(parsed.tables[0].rowCount).toBe(100);
    });
  });

  describe('exportAsSQL', () => {
    it('should generate CREATE TABLE statements', () => {
      const result = exportAsSQL(mockSchema);
      
      expect(result).toContain('CREATE TABLE dbo.Customer');
      expect(result).toContain('CREATE TABLE dbo.Order');
    });

    it('should include column definitions', () => {
      const result = exportAsSQL(mockSchema);
      
      expect(result).toContain('CustomerId int NOT NULL PRIMARY KEY');
      expect(result).toContain('Name nvarchar(100) NOT NULL');
    });

    it('should include foreign key constraints', () => {
      const result = exportAsSQL(mockSchema);
      
      expect(result).toContain('ALTER TABLE Order');
      expect(result).toContain('FOREIGN KEY');
      expect(result).toContain('REFERENCES Customer');
    });

    it('should include DDL header comments', () => {
      const result = exportAsSQL(mockSchema);
      
      expect(result).toContain('-- Database Schema DDL');
      expect(result).toContain('-- Total Tables: 2');
    });

    it('should include table comments', () => {
      const result = exportAsSQL(mockSchema);
      
      expect(result).toContain('-- Table: dbo.Customer');
      expect(result).toContain('-- Rows: 100');
      expect(result).toContain('-- Contains TargCC special columns');
    });
  });

  describe('exportAsMarkdown', () => {
    it('should generate markdown documentation', () => {
      const result = exportAsMarkdown(mockSchema);
      
      expect(result).toContain('# Database Schema Documentation');
      expect(result).toContain('## Customer');
      expect(result).toContain('## Order');
    });

    it('should include table of contents', () => {
      const result = exportAsMarkdown(mockSchema);
      
      expect(result).toContain('## Table of Contents');
      expect(result).toContain('[dbo.Customer]');
      expect(result).toContain('[dbo.Order]');
    });

    it('should include column tables', () => {
      const result = exportAsMarkdown(mockSchema);
      
      expect(result).toContain('### Columns');
      expect(result).toContain('| Column | Type | Nullable | Keys | Default |');
      expect(result).toContain('| CustomerId | int | No | PK | - |');
    });

    it('should include relationship table', () => {
      const result = exportAsMarkdown(mockSchema);
      
      expect(result).toContain('## Relationships');
      expect(result).toContain('| Order | CustomerId | Customer | CustomerId | one-to-many |');
    });

    it('should show TargCC indicator', () => {
      const result = exportAsMarkdown(mockSchema);
      
      expect(result).toContain('**TargCC Columns:** Yes ✓');
    });

    it('should format row counts', () => {
      const result = exportAsMarkdown(mockSchema);
      
      expect(result).toContain('**Row Count:** 100');
    });
  });
});
