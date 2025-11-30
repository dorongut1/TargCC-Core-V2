import type { DatabaseSchema, Table, Column } from '../types/schema';

/**
 * Schema Export Utilities
 * Provides functions to export database schema in various formats
 */

/**
 * Export schema as formatted JSON
 */
export function exportAsJSON(schema: DatabaseSchema): string {
  return JSON.stringify(schema, null, 2);
}

/**
 * Export schema as SQL DDL (Data Definition Language)
 */
export function exportAsSQL(schema: DatabaseSchema): string {
  let sql = '-- Database Schema DDL\n';
  sql += `-- Generated: ${new Date().toISOString()}\n`;
  sql += `-- Total Tables: ${schema.tables.length}\n\n`;

  schema.tables.forEach((table) => {
    sql += generateTableDDL(table);
    sql += '\n';
  });

  // Add foreign key constraints
  sql += '-- Foreign Key Constraints\n';
  schema.relationships.forEach((rel) => {
    const constraintName = `FK_${rel.fromTable}_${rel.toTable}`;
    sql += `ALTER TABLE ${rel.fromTable}\n`;
    sql += `  ADD CONSTRAINT ${constraintName}\n`;
    sql += `  FOREIGN KEY (${rel.fromColumn})\n`;
    sql += `  REFERENCES ${rel.toTable}(${rel.toColumn});\n\n`;
  });

  return sql;
}

/**
 * Generate CREATE TABLE statement for a single table
 */
function generateTableDDL(table: Table): string {
  let sql = `-- Table: ${table.schema}.${table.name}\n`;
  if (table.rowCount) {
    sql += `-- Rows: ${table.rowCount.toLocaleString()}\n`;
  }
  if (table.hasTargCCColumns) {
    sql += `-- Contains TargCC special columns\n`;
  }
  sql += `CREATE TABLE ${table.schema}.${table.name} (\n`;

  const columnDefs = table.columns.map((col) => generateColumnDDL(col));
  sql += columnDefs.join(',\n');

  sql += '\n);\n';
  return sql;
}

/**
 * Generate column definition for DDL
 */
function generateColumnDDL(col: Column): string {
  let def = `  ${col.name} ${col.type}`;

  if (col.maxLength) {
    def += `(${col.maxLength})`;
  }

  if (!col.nullable) {
    def += ' NOT NULL';
  }

  if (col.isPrimaryKey) {
    def += ' PRIMARY KEY';
  }

  if (col.defaultValue) {
    def += ` DEFAULT ${col.defaultValue}`;
  }

  return def;
}

/**
 * Export schema as Markdown documentation
 */
export function exportAsMarkdown(schema: DatabaseSchema): string {
  let md = '# Database Schema Documentation\n\n';
  md += `**Generated:** ${new Date().toLocaleString()}\n\n`;
  md += `**Total Tables:** ${schema.tables.length}\n`;
  md += `**Total Relationships:** ${schema.relationships.length}\n\n`;

  md += '## Table of Contents\n\n';
  schema.tables.forEach((table) => {
    md += `- [${table.schema}.${table.name}](#${table.name.toLowerCase()})\n`;
  });
  md += '\n---\n\n';

  schema.tables.forEach((table) => {
    md += generateTableMarkdown(table);
    md += '\n';
  });

  if (schema.relationships.length > 0) {
    md += '## Relationships\n\n';
    md += '| From Table | From Column | To Table | To Column | Type |\n';
    md += '|------------|-------------|----------|-----------|------|\n';

    schema.relationships.forEach((rel) => {
      md += `| ${rel.fromTable} | ${rel.fromColumn} | ${rel.toTable} | ${rel.toColumn} | ${rel.type} |\n`;
    });
    md += '\n';
  }

  return md;
}

/**
 * Generate Markdown documentation for a single table
 */
function generateTableMarkdown(table: Table): string {
  let md = `## ${table.name}\n\n`;
  md += `**Schema:** ${table.schema}\n`;
  
  if (table.rowCount) {
    md += `**Row Count:** ${table.rowCount.toLocaleString()}\n`;
  }
  
  if (table.hasTargCCColumns) {
    md += `**TargCC Columns:** Yes ✓\n`;
  }
  
  md += '\n### Columns\n\n';
  md += '| Column | Type | Nullable | Keys | Default |\n';
  md += '|--------|------|----------|------|--------|\n';

  table.columns.forEach((col) => {
    const keys = [];
    if (col.isPrimaryKey) keys.push('PK');
    if (col.isForeignKey) keys.push('FK');

    const type = col.maxLength ? `${col.type}(${col.maxLength})` : col.type;
    const nullable = col.nullable ? 'Yes' : 'No';
    const keyStr = keys.length > 0 ? keys.join(', ') : '-';
    const defaultVal = col.defaultValue || '-';

    md += `| ${col.name} | ${type} | ${nullable} | ${keyStr} | ${defaultVal} |\n`;
  });

  md += '\n';
  return md;
}
