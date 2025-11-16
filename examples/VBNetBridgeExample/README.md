# VB.NET Bridge Example

## Overview

This example demonstrates how to use **TargCC.Bridge** from a VB.NET Framework application. It shows the bridge in action, connecting legacy VB.NET code to the modern C# .NET 8 analyzers.

## What This Example Shows

1. ✅ **Basic Usage** - Creating a wrapper and analyzing a database
2. ✅ **Table Filtering** - Separating system tables from user tables
3. ✅ **Column Analysis** - Examining columns, indexes, and relationships
4. ✅ **Model Converter** - Using utility functions for type conversions
5. ✅ **Error Handling** - Proper exception handling and validation

## Requirements

- .NET Framework 4.8
- SQL Server (local or remote)
- TargCC.Bridge.dll
- TargCC.Core.Interfaces.dll

## How to Run

### Option 1: Visual Studio

1. Open `VBNetBridgeExample.vbproj` in Visual Studio
2. Update connection string in `Program.vb` if needed:
   ```vb
   Dim connectionString = "Server=.;Database=tempdb;Integrated Security=true;TrustServerCertificate=true"
   ```
3. Build and Run (F5)

### Option 2: Command Line

```bash
cd C:\Disk1\TargCC-Core-V2\examples\VBNetBridgeExample
dotnet build
dotnet run
```

## Code Walkthrough

### Example 1: Basic Usage

```vb
' Create wrapper
Dim wrapper As New DatabaseAnalyzerWrapper(connectionString)

' Test connection
If wrapper.TestConnection() Then
    ' Get tables
    Dim tables = wrapper.GetTables()
    Console.WriteLine($"Found {tables.Count} tables")
End If
```

### Example 2: Table Filtering

```vb
' Get system tables only (start with 'c_')
Dim systemTables = wrapper.GetSystemTables()

' Get user tables only
Dim userTables = wrapper.GetUserTables()

' Get specific table
Dim customerTable = wrapper.GetTable("Customer")
```

### Example 3: Column Analysis

```vb
' Get first table
Dim table = tables(0)

' Examine columns
For Each column In table.Columns
    Console.WriteLine($"{column.Name} {column.DataType}")
    
    ' Check prefix
    Dim prefix = ModelConverter.PrefixToString(column.Prefix)
    If Not String.IsNullOrEmpty(prefix) Then
        Console.WriteLine($"  Prefix: {prefix}")
    End If
Next

' Examine indexes
For Each index In table.Indexes
    Console.WriteLine($"Index: {index.Name}")
Next

' Examine relationships
For Each rel In table.Relationships
    Console.WriteLine($"FK: {rel.ForeignKeyColumn} -> {rel.ParentTable}")
Next
```

### Example 4: Model Converter

```vb
' Convert prefix enum to string
Dim str = ModelConverter.PrefixToString(ColumnPrefix.Eno)  ' Returns "eno"

' Convert string to prefix enum
Dim prefix = ModelConverter.StringToPrefix("ent")  ' Returns ColumnPrefix.Ent

' Get table full name
Dim fullName = ModelConverter.GetTableFullName(table)  ' Returns "dbo.Customer"

' Check if column is primary key
Dim isPK = ModelConverter.IsPrimaryKey(column, table)

' Get foreign key columns
Dim fkColumns = ModelConverter.GetForeignKeyColumns(table)
```

### Example 5: Error Handling

```vb
' Handle invalid connection
Try
    Dim wrapper As New DatabaseAnalyzerWrapper(Nothing)
Catch ex As ArgumentException
    Console.WriteLine("Invalid connection string")
End Try

' Handle connection failure
Dim wrapper As New DatabaseAnalyzerWrapper("Server=BadServer;...")
If Not wrapper.TestConnection() Then
    Console.WriteLine("Cannot connect")
End If

' Handle missing table
Try
    Dim table = wrapper.GetTable(Nothing)
Catch ex As ArgumentException
    Console.WriteLine("Table name required")
End Try
```

## Expected Output

```
===========================================
TargCC.Bridge Example - VB.NET Framework
===========================================

Example 1: Basic Usage
----------------------
Creating wrapper with connection: Server=.;Database=tempdb;...
Testing connection... SUCCESS!
Found 42 tables in database

Example 2: Table Filtering
--------------------------
Total tables: 42
System tables: 5
User tables: 37

First 5 tables:
  [SYSTEM] dbo.c_SystemDefault
  [SYSTEM] dbo.c_Table
  [USER]   dbo.Customer
  [USER]   dbo.Order
  [USER]   dbo.Product

Example 3: Column Analysis
--------------------------
Analyzing table: dbo.Customer

Columns (8):
  ID                             bigint          NOT NULL
  Name                           nvarchar        NOT NULL
  enoPassword                    varchar         NOT NULL [eno]
  entCreditCard                  varchar         NULL [ent]
  enmStatus                      int             NOT NULL [enm]
  CreatedDate                    datetime        NOT NULL
  clc_TotalOrders                int             NOT NULL [clc_]
  spt_Notes                      nvarchar        NULL [spt_]

Indexes (2):
  UNIQUE PK_Customer: (ID)
         IX_Customer_Name: (Name)

Relationships (0):

Example 4: Model Converter
--------------------------
Column Prefix Conversions:
  Eno             -> 'eno'
  Ent             -> 'ent'
  Enm             -> 'enm'
  ...

String to Prefix Conversions:
  'eno'       -> Eno
  'ent'       -> Ent
  ...

Example 5: Error Handling
-------------------------
Test 1: Invalid connection string... OK (ArgumentException caught)
Test 2: Null connection string... OK (ArgumentException caught)
Test 3: Non-existent server... OK (TestConnection returned False)
Test 4: GetTable with null name... OK (ArgumentException caught)

Press any key to exit...
```

## Migration from Old Code

### Old VB.NET Code (using DBAnalyser)

```vb
Imports DBAnalyser

Dim db As New clsDatabase(connectionString)
Dim tables = db.GetTables()

For Each table As clsTable In tables
    Console.WriteLine(table.Name)
    For Each column As clsColumn In table.Columns
        Console.WriteLine($"  {column.Name}")
    Next
Next
```

### New Code (using Bridge)

```vb
Imports TargCC.Bridge.Wrappers

Dim wrapper As New DatabaseAnalyzerWrapper(connectionString)
Dim tables = wrapper.GetTables()

For Each table In tables
    Console.WriteLine(table.Name)
    For Each column In table.Columns
        Console.WriteLine($"  {column.Name}")
    Next
Next
```

**Changes:**
1. Import statement: `DBAnalyser` → `TargCC.Bridge.Wrappers`
2. Class name: `clsDatabase` → `DatabaseAnalyzerWrapper`
3. Type names: `clsTable` → `Table`, `clsColumn` → `Column`

**That's it!** The rest of the code remains identical.

## Troubleshooting

### Cannot connect to SQL Server

- Verify SQL Server is running
- Check connection string
- Ensure SQL Server accepts TCP/IP connections
- Check firewall settings

### Missing DLLs

- Ensure TargCC.Bridge.dll is in output directory
- Ensure TargCC.Core.Interfaces.dll is referenced
- Build the Bridge project first

### Build Errors

- Ensure .NET Framework 4.8 SDK is installed
- Clean and rebuild solution
- Check project references

## Support

For issues or questions:
- Check TargCC.Bridge README.md
- Review API documentation
- Contact TargCC development team

---

**Version**: 1.0.0  
**Target Framework**: .NET Framework 4.8  
**Language**: VB.NET  
**License**: Proprietary - Target Systems
