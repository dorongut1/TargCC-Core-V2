' <copyright file="Program.vb" company="Target Systems">
' Copyright (c) Target Systems. All rights reserved.
' </copyright>

Imports System
Imports System.Collections.Generic
Imports TargCC.Bridge.Wrappers
Imports TargCC.Core.Interfaces

''' <summary>
''' Example application demonstrating TargCC.Bridge usage.
''' Shows how legacy VB.NET Framework code can use the new C# analyzers.
''' </summary>
Module Program
    ''' <summary>
    ''' Main entry point.
    ''' </summary>
    Sub Main(args As String())
        Console.WriteLine("===========================================")
        Console.WriteLine("TargCC.Bridge Example - VB.NET Framework")
        Console.WriteLine("===========================================")
        Console.WriteLine()

        ' Example 1: Basic Usage
        BasicUsageExample()
        Console.WriteLine()

        ' Example 2: Table Filtering
        TableFilteringExample()
        Console.WriteLine()

        ' Example 3: Column Analysis
        ColumnAnalysisExample()
        Console.WriteLine()

        ' Example 4: Error Handling
        ErrorHandlingExample()
        Console.WriteLine()

        Console.WriteLine("Press any key to exit...")
        Console.ReadKey()
    End Sub

    ''' <summary>
    ''' Example 1: Basic wrapper usage.
    ''' </summary>
    Private Sub BasicUsageExample()
        Console.WriteLine("Example 1: Basic Usage")
        Console.WriteLine("----------------------")

        Try
            ' Create connection string (modify for your environment)
            Dim connectionString = "Server=.;Database=tempdb;Integrated Security=true;TrustServerCertificate=true"

            ' Create wrapper
            Console.WriteLine($"Creating wrapper with connection: {connectionString}")
            Dim wrapper As New DatabaseAnalyzerWrapper(connectionString)

            ' Test connection
            Console.Write("Testing connection... ")
            If wrapper.TestConnection() Then
                Console.WriteLine("SUCCESS!")

                ' Get tables
                Dim tables = wrapper.GetTables()
                Console.WriteLine($"Found {tables.Count} tables in database")
            Else
                Console.WriteLine("FAILED!")
                Console.WriteLine("Could not connect to database. Please check connection string.")
            End If

        Catch ex As Exception
            Console.WriteLine($"ERROR: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Example 2: Filtering system vs user tables.
    ''' </summary>
    Private Sub TableFilteringExample()
        Console.WriteLine("Example 2: Table Filtering")
        Console.WriteLine("--------------------------")

        Try
            Dim connectionString = "Server=.;Database=tempdb;Integrated Security=true;TrustServerCertificate=true"
            Dim wrapper As New DatabaseAnalyzerWrapper(connectionString)

            If Not wrapper.TestConnection() Then
                Console.WriteLine("Cannot connect to database. Skipping example.")
                Return
            End If

            ' Get all tables
            Dim allTables = wrapper.GetTables()
            Console.WriteLine($"Total tables: {allTables.Count}")

            ' Get only system tables (starting with c_)
            Dim systemTables = wrapper.GetSystemTables()
            Console.WriteLine($"System tables: {systemTables.Count}")

            ' Get only user tables
            Dim userTables = wrapper.GetUserTables()
            Console.WriteLine($"User tables: {userTables.Count}")

            ' Show first 5 tables
            If allTables.Count > 0 Then
                Console.WriteLine()
                Console.WriteLine("First 5 tables:")
                For i As Integer = 0 To Math.Min(4, allTables.Count - 1)
                    Dim table = allTables(i)
                    Dim tableType = If(table.IsSystemTable, "[SYSTEM]", "[USER]  ")
                    Console.WriteLine($"  {tableType} {table.Schema}.{table.Name}")
                Next
            End If

        Catch ex As Exception
            Console.WriteLine($"ERROR: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Example 3: Analyzing columns in a specific table.
    ''' </summary>
    Private Sub ColumnAnalysisExample()
        Console.WriteLine("Example 3: Column Analysis")
        Console.WriteLine("--------------------------")

        Try
            Dim connectionString = "Server=.;Database=tempdb;Integrated Security=true;TrustServerCertificate=true"
            Dim wrapper As New DatabaseAnalyzerWrapper(connectionString)

            If Not wrapper.TestConnection() Then
                Console.WriteLine("Cannot connect to database. Skipping example.")
                Return
            End If

            ' Get first table
            Dim tables = wrapper.GetTables()
            If tables.Count = 0 Then
                Console.WriteLine("No tables found in database.")
                Return
            End If

            Dim firstTable = tables(0)
            Console.WriteLine($"Analyzing table: {firstTable.Schema}.{firstTable.Name}")
            Console.WriteLine()

            ' Show columns
            Console.WriteLine($"Columns ({firstTable.Columns.Count}):")
            For Each column In firstTable.Columns
                Dim nullable = If(column.IsNullable, "NULL", "NOT NULL")
                Dim prefixStr = If(column.Prefix = ColumnPrefix.None, "", $" [{column.Prefix}]")

                Console.WriteLine($"  {column.Name,-30} {column.DataType,-15} {nullable}{prefixStr}")
            Next

            ' Show indexes
            If firstTable.Indexes.Count > 0 Then
                Console.WriteLine()
                Console.WriteLine($"Indexes ({firstTable.Indexes.Count}):")
                For Each index In firstTable.Indexes
                    Dim indexType = If(index.IsUnique, "UNIQUE", "      ")
                    Dim columnList = String.Join(", ", index.Columns)
                    Console.WriteLine($"  {indexType} {index.Name}: ({columnList})")
                Next
            End If

            ' Show relationships
            If firstTable.Relationships.Count > 0 Then
                Console.WriteLine()
                Console.WriteLine($"Relationships ({firstTable.Relationships.Count}):")
                For Each rel In firstTable.Relationships
                    Console.WriteLine($"  {rel.RelationType}: {rel.ForeignKeyColumn} -> {rel.ParentTable}.{rel.ParentKeyColumn}")
                Next
            End If

        Catch ex As Exception
            Console.WriteLine($"ERROR: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Example 4: Error handling.
    ''' </summary>
    Private Sub ErrorHandlingExample()
        Console.WriteLine("Example 4: Error Handling")
        Console.WriteLine("-------------------------")

        ' Test 1: Null connection string
        Console.Write("Test 1: Null connection string... ")
        Try
            Dim wrapper As New DatabaseAnalyzerWrapper(Nothing)
            Console.WriteLine("FAILED (should have thrown exception)")
        Catch ex As ArgumentException
            Console.WriteLine("OK (ArgumentException caught)")
        End Try

        ' Test 2: Empty connection string
        Console.Write("Test 2: Empty connection string... ")
        Try
            Dim wrapper As New DatabaseAnalyzerWrapper(String.Empty)
            Console.WriteLine("FAILED (should have thrown exception)")
        Catch ex As ArgumentException
            Console.WriteLine("OK (ArgumentException caught)")
        End Try

        ' Test 3: Connection to non-existent server
        Console.Write("Test 3: Non-existent server... ")
        Try
            Dim wrapper As New DatabaseAnalyzerWrapper("Server=NonExistentServer;Database=Test")
            Dim result = wrapper.TestConnection()
            If Not result Then
                Console.WriteLine("OK (TestConnection returned False)")
            Else
                Console.WriteLine("UNEXPECTED (connection succeeded)")
            End If
        Catch ex As Exception
            Console.WriteLine($"OK (Exception: {ex.GetType().Name})")
        End Try

        ' Test 4: GetTable with null name
        Console.Write("Test 4: GetTable with null name... ")
        Try
            Dim wrapper As New DatabaseAnalyzerWrapper("Server=.;Database=tempdb")
            Dim table = wrapper.GetTable(Nothing)
            Console.WriteLine("FAILED (should have thrown exception)")
        Catch ex As ArgumentException
            Console.WriteLine("OK (ArgumentException caught)")
        End Try
    End Sub
End Module
