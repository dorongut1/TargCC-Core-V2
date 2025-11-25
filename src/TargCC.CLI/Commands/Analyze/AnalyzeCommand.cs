// <copyright file="AnalyzeCommand.cs" company="Doron Vaida">
// Copyright (c) Doron Vaida. All rights reserved.
// </copyright>

using System.CommandLine;
using Microsoft.Extensions.Logging;
using TargCC.CLI.Configuration;
using TargCC.CLI.Services;
using TargCC.CLI.Services.Analysis;

namespace TargCC.CLI.Commands.Analyze;

/// <summary>
/// Parent command for analysis operations.
/// </summary>
public class AnalyzeCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeCommand"/> class.
    /// </summary>
    /// <param name="loggerFactory">Logger factory.</param>
    /// <param name="configService">Configuration service.</param>
    /// <param name="output">Output service.</param>
    /// <param name="analysisService">Analysis service.</param>
    public AnalyzeCommand(
        ILoggerFactory loggerFactory,
        IConfigurationService configService,
        IOutputService output,
        IAnalysisService analysisService)
        : base("analyze", "Analyze database schema and code impact")
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(configService);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(analysisService);

        // Add subcommands
        this.AddCommand(new AnalyzeSchemaCommand(analysisService, output, loggerFactory));
        this.AddCommand(new AnalyzeImpactCommand(analysisService, output, loggerFactory));
        this.AddCommand(new AnalyzeSecurityCommand(analysisService, output, loggerFactory));
        this.AddCommand(new AnalyzeQualityCommand(analysisService, output, loggerFactory));
    }
}
