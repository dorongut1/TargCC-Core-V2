// <copyright file="EncryptionSuggestion.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Represents a suggestion to encrypt a column containing sensitive data.
/// </summary>
public sealed class EncryptionSuggestion
{
    /// <summary>
    /// Gets the name of the column that should be encrypted.
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// Gets the name of the table containing the column.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Gets the type of sensitive data (e.g., "SSN", "CreditCard", "Password").
    /// </summary>
    public required string SensitiveDataType { get; init; }

    /// <summary>
    /// Gets the recommended encryption method.
    /// </summary>
    public required string RecommendedEncryptionMethod { get; init; }

    /// <summary>
    /// Gets the reason why this column should be encrypted.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Gets the severity/importance of this suggestion.
    /// </summary>
    public required SecuritySeverity Severity { get; init; }

    /// <summary>
    /// Gets the recommended column name with eno_ prefix.
    /// </summary>
    public string? RecommendedColumnName { get; init; }

    /// <summary>
    /// Gets additional implementation notes.
    /// </summary>
    public string? ImplementationNotes { get; init; }
}
