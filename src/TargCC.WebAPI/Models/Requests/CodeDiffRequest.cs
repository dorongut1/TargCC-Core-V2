// <copyright file="CodeDiffRequest.cs" company="TargCC">
// Copyright (c) TargCC. All rights reserved.
// </copyright>

namespace TargCC.WebAPI.Models.Requests;

/// <summary>
/// Request model for generating code diff.
/// </summary>
public sealed class CodeDiffRequest
{
    /// <summary>
    /// Gets or sets the original code.
    /// </summary>
    public string OriginalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the modified code.
    /// </summary>
    public string ModifiedCode { get; set; } = string.Empty;
}
