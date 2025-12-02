// <copyright file="CodeValidationRequest.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request model for code validation.
/// </summary>
public sealed class CodeValidationRequest
{
    /// <summary>
    /// Gets or sets the original code.
    /// </summary>
    public string OriginalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the modified code to validate.
    /// </summary>
    public string ModifiedCode { get; set; } = string.Empty;
}
