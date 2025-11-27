// <copyright file="SchemaAnalysisPromptBuilder.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

using System.Text;
using TargCC.Core.Interfaces.Models;

namespace TargCC.AI.Prompts;

/// <summary>
/// Builds prompts for schema analysis.
/// </summary>
public class SchemaAnalysisPromptBuilder : IPromptBuilder
{
    private readonly Table table;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaAnalysisPromptBuilder"/> class.
    /// </summary>
    /// <param name="table">The table to analyze.</param>
    public SchemaAnalysisPromptBuilder(Table table)
    {
        this.table = table ?? throw new ArgumentNullException(nameof(table));
    }

    /// <inheritdoc/>
    public string Build()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Analyze the following database table schema and provide suggestions:");
        sb.AppendLine();
        sb.AppendLine($"**Table:** {this.table.Name}");
        sb.AppendLine($"**Schema:** {this.table.SchemaName}");
        sb.AppendLine();

        // Columns
        sb.AppendLine("**Columns:**");
        foreach (var column in this.table.Columns)
        {
            sb.AppendLine($"- {column.Name}");
            sb.AppendLine($"  - Type: {column.DataType}");
            sb.AppendLine($"  - Nullable: {column.IsNullable}");
            sb.AppendLine($"  - Primary Key: {column.IsPrimaryKey}");
            sb.AppendLine($"  - Foreign Key: {column.IsForeignKey}");
            if (column.IsForeignKey && !string.IsNullOrEmpty(column.ReferencedTable))
            {
                sb.AppendLine($"  - References: {column.ReferencedTable}");
            }

            sb.AppendLine($"  - Max Length: {column.MaxLength?.ToString() ?? "N/A"}");
        }

        sb.AppendLine();

        // Indexes
        if (this.table.Indexes.Any())
        {
            sb.AppendLine("**Indexes:**");
            foreach (var index in this.table.Indexes)
            {
                sb.AppendLine($"- {index.Name} ({(index.IsUnique ? "Unique" : "Non-Unique")})");
                sb.AppendLine($"  - Columns: {string.Join(", ", index.ColumnNames)}");
            }

            sb.AppendLine();
        }

        // Request structured analysis
        sb.AppendLine("Please provide analysis in the following JSON format:");
        sb.AppendLine("```json");
        sb.AppendLine("{");
        sb.AppendLine("  \"tableName\": \"string\",");
        sb.AppendLine("  \"summary\": \"string\",");
        sb.AppendLine("  \"qualityScore\": 0-100,");
        sb.AppendLine("  \"followsTargCCConventions\": boolean,");
        sb.AppendLine("  \"strengths\": [\"string\"],");
        sb.AppendLine("  \"issues\": [\"string\"],");
        sb.AppendLine("  \"suggestions\": [");
        sb.AppendLine("    {");
        sb.AppendLine("      \"severity\": \"Info|BestPractice|Warning|Critical\",");
        sb.AppendLine("      \"category\": \"General|Security|Performance|Naming|Relationships|Indexing|DataType|TargCCConventions\",");
        sb.AppendLine("      \"message\": \"string\",");
        sb.AppendLine("      \"target\": \"column or table name\",");
        sb.AppendLine("      \"recommendedAction\": \"string\",");
        sb.AppendLine("      \"context\": \"string\"");
        sb.AppendLine("    }");
        sb.AppendLine("  ]");
        sb.AppendLine("}");
        sb.AppendLine("```");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string GetSystemMessage()
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an expert database schema analyzer for TargCC, a code generation system.");
        sb.AppendLine("Your role is to analyze database table schemas and provide actionable suggestions.");
        sb.AppendLine();
        sb.AppendLine("**TargCC Conventions:**");
        sb.AppendLine();
        sb.AppendLine("1. **eno_** prefix = Encrypted columns (SSN, CreditCard, etc.)");
        sb.AppendLine("   - These columns contain sensitive data that must be encrypted");
        sb.AppendLine("   - Examples: eno_SSN, eno_CreditCard, eno_Password");
        sb.AppendLine();
        sb.AppendLine("2. **ent_** prefix = Temporal columns (CreatedDate, ModifiedDate, etc.)");
        sb.AppendLine("   - Track when records are created/modified");
        sb.AppendLine("   - Examples: ent_CreatedDate, ent_ModifiedDate, ent_DeletedDate");
        sb.AppendLine();
        sb.AppendLine("3. **clc_** prefix = Calculated columns");
        sb.AppendLine("   - Computed values, not stored directly");
        sb.AppendLine("   - Examples: clc_TotalPrice, clc_Age, clc_FullName");
        sb.AppendLine();
        sb.AppendLine("4. **blg_** prefix = Boolean logic columns");
        sb.AppendLine("   - Binary yes/no flags");
        sb.AppendLine("   - Examples: blg_IsActive, blg_IsDeleted, blg_IsApproved");
        sb.AppendLine();
        sb.AppendLine("5. **agg_** prefix = Aggregate columns");
        sb.AppendLine("   - Aggregated data from related tables");
        sb.AppendLine("   - Examples: agg_TotalOrders, agg_SumAmount");
        sb.AppendLine();
        sb.AppendLine("6. **spt_** prefix = Split columns");
        sb.AppendLine("   - Data split from other columns");
        sb.AppendLine("   - Examples: spt_FirstName, spt_LastName (from FullName)");
        sb.AppendLine();
        sb.AppendLine("**Best Practices to Check:**");
        sb.AppendLine("- Table names should be singular (Customer, not Customers)");
        sb.AppendLine("- Primary keys should exist and be properly named");
        sb.AppendLine("- Foreign key relationships should be defined");
        sb.AppendLine("- Sensitive data should have eno_ prefix");
        sb.AppendLine("- Temporal columns should have ent_ prefix");
        sb.AppendLine("- Proper indexes on frequently queried columns");
        sb.AppendLine("- Appropriate data types and lengths");
        sb.AppendLine("- Nullable columns should be justified");
        sb.AppendLine();
        sb.AppendLine("Provide concise, actionable suggestions with clear severity levels.");

        return sb.ToString();
    }
}
