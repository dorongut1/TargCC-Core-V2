/**
 * Database Schema Type Definitions
 * Represents database structure for schema visualization
 */

/**
 * Represents a database column
 */
export interface Column {
  /** Column name */
  name: string;
  /** SQL data type (int, nvarchar, datetime2, etc.) */
  type: string;
  /** Whether the column allows NULL values */
  nullable: boolean;
  /** Whether this column is a primary key */
  isPrimaryKey: boolean;
  /** Whether this column is a foreign key */
  isForeignKey: boolean;
  /** Referenced table name if foreign key */
  foreignKeyTable?: string;
  /** Referenced column name if foreign key */
  foreignKeyColumn?: string;
  /** Maximum length for string types */
  maxLength?: number;
  /** Default value if specified */
  defaultValue?: string;
}

/**
 * Represents a database table
 */
export interface Table {
  /** Table name */
  name: string;
  /** Schema name (dbo, etc.) */
  schema: string;
  /** List of columns in the table */
  columns: Column[];
  /** Number of rows in the table */
  rowCount?: number;
  /** Whether table contains TargCC special columns */
  hasTargCCColumns: boolean;
}

/**
 * Represents a relationship between tables
 */
export interface Relationship {
  /** Source table name */
  fromTable: string;
  /** Source column name */
  fromColumn: string;
  /** Target table name */
  toTable: string;
  /** Target column name */
  toColumn: string;
  /** Relationship type */
  type: 'one-to-one' | 'one-to-many' | 'many-to-many';
}

/**
 * Represents complete database schema
 */
export interface DatabaseSchema {
  /** All tables in the database */
  tables: Table[];
  /** All relationships between tables */
  relationships: Relationship[];
}
