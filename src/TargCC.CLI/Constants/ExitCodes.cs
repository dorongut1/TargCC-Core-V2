// <copyright file="ExitCodes.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

namespace TargCC.CLI.Constants;

/// <summary>
/// Standard exit codes for CLI commands.
/// </summary>
public static class ExitCodes
{
    /// <summary>
    /// Command executed successfully.
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// General error occurred.
    /// </summary>
    public const int GeneralError = 1;

    /// <summary>
    /// Configuration error (missing or invalid configuration).
    /// </summary>
    public const int ConfigurationError = 2;

    /// <summary>
    /// Database connection error.
    /// </summary>
    public const int DatabaseError = 3;

    /// <summary>
    /// File system error (cannot read/write files).
    /// </summary>
    public const int FileSystemError = 4;

    /// <summary>
    /// Invalid arguments provided.
    /// </summary>
    public const int InvalidArguments = 5;

    /// <summary>
    /// Operation cancelled by user.
    /// </summary>
    public const int Cancelled = 6;

    /// <summary>
    /// Table or schema not found.
    /// </summary>
    public const int NotFound = 7;

    /// <summary>
    /// Generation failed.
    /// </summary>
    public const int GenerationFailed = 8;
}
