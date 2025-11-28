// <copyright file="SecuritySeverity.cs" company="Doron Aharoni">
// Copyright (c) Doron Aharoni. All rights reserved.
// </copyright>

namespace TargCC.AI.Models;

/// <summary>
/// Represents the severity level of a security issue.
/// </summary>
public enum SecuritySeverity
{
    /// <summary>
    /// Low severity - minor security concern.
    /// </summary>
    Low,

    /// <summary>
    /// Medium severity - moderate security concern.
    /// </summary>
    Medium,

    /// <summary>
    /// High severity - significant security concern.
    /// </summary>
    High,

    /// <summary>
    /// Critical severity - critical security vulnerability.
    /// </summary>
    Critical,
}
