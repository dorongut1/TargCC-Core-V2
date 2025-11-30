import type { DatabaseSchema } from '../types/schema';

/**
 * Mock database schema for development and testing
 * Includes realistic table structures with TargCC columns
 */
export const mockSchema: DatabaseSchema = {
  tables: [
    {
      name: 'Customer',
      schema: 'dbo',
      rowCount: 1250,
      hasTargCCColumns: true,
      columns: [
        { 
          name: 'CustomerId', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: true, 
          isForeignKey: false 
        },
        { 
          name: 'eno_FirstName', 
          type: 'nvarchar', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false, 
          maxLength: 50 
        },
        { 
          name: 'eno_LastName', 
          type: 'nvarchar', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false, 
          maxLength: 50 
        },
        { 
          name: 'eno_Email', 
          type: 'nvarchar', 
          nullable: true, 
          isPrimaryKey: false, 
          isForeignKey: false, 
          maxLength: 100 
        },
        { 
          name: 'Phone', 
          type: 'nvarchar', 
          nullable: true, 
          isPrimaryKey: false, 
          isForeignKey: false, 
          maxLength: 20 
        },
        { 
          name: 'ent_CreatedDate', 
          type: 'datetime2', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
        { 
          name: 'ent_ModifiedDate', 
          type: 'datetime2', 
          nullable: true, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
      ]
    },
    {
      name: 'Order',
      schema: 'dbo',
      rowCount: 5430,
      hasTargCCColumns: true,
      columns: [
        { 
          name: 'OrderId', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: true, 
          isForeignKey: false 
        },
        { 
          name: 'CustomerId', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: true,
          foreignKeyTable: 'Customer',
          foreignKeyColumn: 'CustomerId'
        },
        { 
          name: 'OrderDate', 
          type: 'datetime2', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
        { 
          name: 'clc_TotalAmount', 
          type: 'decimal', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
        { 
          name: 'Status', 
          type: 'nvarchar', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false,
          maxLength: 20,
          defaultValue: 'Pending'
        },
        { 
          name: 'ent_CreatedDate', 
          type: 'datetime2', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
        { 
          name: 'ent_ModifiedDate', 
          type: 'datetime2', 
          nullable: true, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
      ]
    },
    {
      name: 'OrderItem',
      schema: 'dbo',
      rowCount: 18920,
      hasTargCCColumns: true,
      columns: [
        { 
          name: 'OrderItemId', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: true, 
          isForeignKey: false 
        },
        { 
          name: 'OrderId', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: true,
          foreignKeyTable: 'Order',
          foreignKeyColumn: 'OrderId'
        },
        { 
          name: 'ProductId', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: true,
          foreignKeyTable: 'Product',
          foreignKeyColumn: 'ProductId'
        },
        { 
          name: 'Quantity', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
        { 
          name: 'clc_UnitPrice', 
          type: 'decimal', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
        { 
          name: 'clc_LineTotal', 
          type: 'decimal', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
      ]
    },
    {
      name: 'Product',
      schema: 'dbo',
      rowCount: 342,
      hasTargCCColumns: true,
      columns: [
        { 
          name: 'ProductId', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: true, 
          isForeignKey: false 
        },
        { 
          name: 'eno_Name', 
          type: 'nvarchar', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false,
          maxLength: 100
        },
        { 
          name: 'Description', 
          type: 'nvarchar', 
          nullable: true, 
          isPrimaryKey: false, 
          isForeignKey: false,
          maxLength: 500
        },
        { 
          name: 'CategoryId', 
          type: 'int', 
          nullable: true, 
          isPrimaryKey: false, 
          isForeignKey: true,
          foreignKeyTable: 'Category',
          foreignKeyColumn: 'CategoryId'
        },
        { 
          name: 'Price', 
          type: 'decimal', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false 
        },
        { 
          name: 'StockQuantity', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false,
          defaultValue: '0'
        },
        { 
          name: 'IsActive', 
          type: 'bit', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false,
          defaultValue: '1'
        },
      ]
    },
    {
      name: 'Category',
      schema: 'dbo',
      rowCount: 25,
      hasTargCCColumns: false,
      columns: [
        { 
          name: 'CategoryId', 
          type: 'int', 
          nullable: false, 
          isPrimaryKey: true, 
          isForeignKey: false 
        },
        { 
          name: 'Name', 
          type: 'nvarchar', 
          nullable: false, 
          isPrimaryKey: false, 
          isForeignKey: false,
          maxLength: 50
        },
        { 
          name: 'Description', 
          type: 'nvarchar', 
          nullable: true, 
          isPrimaryKey: false, 
          isForeignKey: false,
          maxLength: 200
        },
      ]
    },
  ],
  relationships: [
    {
      fromTable: 'Order',
      fromColumn: 'CustomerId',
      toTable: 'Customer',
      toColumn: 'CustomerId',
      type: 'one-to-many'
    },
    {
      fromTable: 'OrderItem',
      fromColumn: 'OrderId',
      toTable: 'Order',
      toColumn: 'OrderId',
      type: 'one-to-many'
    },
    {
      fromTable: 'OrderItem',
      fromColumn: 'ProductId',
      toTable: 'Product',
      toColumn: 'ProductId',
      type: 'one-to-many'
    },
    {
      fromTable: 'Product',
      fromColumn: 'CategoryId',
      toTable: 'Category',
      toColumn: 'CategoryId',
      type: 'one-to-many'
    },
  ]
};
