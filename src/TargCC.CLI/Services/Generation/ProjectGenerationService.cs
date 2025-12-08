using Microsoft.Extensions.Logging;
using TargCC.CLI.Services;
using TargCC.Core.Analyzers.Database;
using TargCC.Core.Generators.Project;
using TargCC.Core.Generators.Project.Models;
using TargCC.Core.Generators.Entities;
using TargCC.Core.Generators.Sql;
using TargCC.Core.Generators.Repositories;
using TargCC.Core.Generators.API;
using TargCC.Core.Generators.Common;
using TargCC.Core.Generators.UI;
using TargCC.Core.Generators.UI.Components;
using TargCC.Core.Interfaces.Models;

namespace TargCC.CLI.Services.Generation;

/// <summary>
/// Service for generating complete Clean Architecture projects.
/// </summary>
public class ProjectGenerationService : IProjectGenerationService
{
    private readonly ILogger<ProjectGenerationService> _logger;
    private readonly IOutputService _output;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISolutionGenerator _solutionGenerator;
    private readonly IProjectStructureGenerator _structureGenerator;
    private readonly IProjectFileGenerator _projectFileGenerator;
    private readonly IProgramCsGenerator _programCsGenerator;
    private readonly IAppSettingsGenerator _appSettingsGenerator;
    private readonly IDependencyInjectionGenerator _diGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectGenerationService"/> class.
    /// </summary>
    public ProjectGenerationService(
        ILogger<ProjectGenerationService> logger,
        IOutputService output,
        ILoggerFactory loggerFactory,
        ISolutionGenerator solutionGenerator,
        IProjectStructureGenerator structureGenerator,
        IProjectFileGenerator projectFileGenerator,
        IProgramCsGenerator programCsGenerator,
        IAppSettingsGenerator appSettingsGenerator,
        IDependencyInjectionGenerator diGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _solutionGenerator = solutionGenerator ?? throw new ArgumentNullException(nameof(solutionGenerator));
        _structureGenerator = structureGenerator ?? throw new ArgumentNullException(nameof(structureGenerator));
        _projectFileGenerator = projectFileGenerator ?? throw new ArgumentNullException(nameof(projectFileGenerator));
        _programCsGenerator = programCsGenerator ?? throw new ArgumentNullException(nameof(programCsGenerator));
        _appSettingsGenerator = appSettingsGenerator ?? throw new ArgumentNullException(nameof(appSettingsGenerator));
        _diGenerator = diGenerator ?? throw new ArgumentNullException(nameof(diGenerator));
    }

    /// <inheritdoc/>
    public async Task GenerateCompleteProjectAsync(
        string databaseName,
        string connectionString,
        string outputDirectory,
        string rootNamespace,
        bool includeTests,
        bool force)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootNamespace);

        try
        {
            _output.Info("Step 1: Analyzing database schema...");

            // Get all tables from database
            var analyzerLogger = _loggerFactory.CreateLogger<DatabaseAnalyzer>();
            var analyzer = new DatabaseAnalyzer(connectionString, analyzerLogger);
            var schema = await analyzer.AnalyzeAsync();
            var tables = schema.Tables.ToList();

            _output.Info($"  ✓ Found {tables.Count} tables");
            _output.Info($"  ✓ Found {schema.Relationships?.Count ?? 0} relationships");
            _output.BlankLine();

            _output.Info("Step 2: Creating solution structure...");

            // Generate solution structure
            var projectOptions = new ProjectGenerationOptions
            {
                ProjectName = rootNamespace,
                RootNamespace = rootNamespace,
                OutputDirectory = outputDirectory,
                ConnectionString = connectionString,
                IncludeTests = includeTests
            };

            await GenerateSolutionStructureAsync(projectOptions);

            _output.Info("  ✓ Solution structure created!");
            _output.BlankLine();

            _output.Info($"Step 3: Generating from {tables.Count} tables...");

            // Generate for each table
            int totalFiles = 0;
            foreach (var table in tables)
            {
                _output.Info($"  Processing: {table.Name}");
                var filesGenerated = await GenerateForTableAsync(
                    table,
                    schema,
                    outputDirectory,
                    rootNamespace);
                totalFiles += filesGenerated;
            }

            _output.Info($"  ✓ Generated {totalFiles} files from {tables.Count} tables!");
            _output.BlankLine();

            _output.Info("Step 3.5: Generating SQL stored procedures with Master-Detail relationships...");
            _output.Info($"  Schema has {schema.Relationships?.Count ?? 0} relationships for Master-Detail generation");

            // Generate ALL SQL including Master-Detail SPs for entire schema
            var sqlGen = new SqlGenerator(_loggerFactory.CreateLogger<SqlGenerator>());
            var allSql = await sqlGen.GenerateAsync(schema);
            var sqlPath = Path.Combine(outputDirectory, "sql", "all_procedures.sql");
            await SaveFileAsync(sqlPath, allSql);

            _output.Info($"  ✓ SQL file generated with {allSql.Length} characters");
            if (schema.Relationships != null && schema.Relationships.Count > 0)
            {
                _output.Info($"  ✓ Included Master-Detail stored procedures for {schema.Relationships.Count} relationships");
            }
            else
            {
                _output.Warning("  ⚠ No relationships found - Master-Detail SPs were NOT generated!");
            }
            _output.BlankLine();

            _output.Info("Step 4: Generating support files...");

            // Generate Program.cs
            await GenerateProgramCsAsync(outputDirectory, rootNamespace, connectionString, tables);

            // Generate appsettings.json
            await GenerateAppSettingsAsync(outputDirectory, rootNamespace, connectionString);

            // Generate DI registration
            await GenerateDependencyInjectionAsync(outputDirectory, rootNamespace, tables);

            _output.Info("  ✓ Support files generated!");
            _output.BlankLine();

            _output.Info("Step 5: Generating React Frontend Setup...");

            // Generate React setup files
            await GenerateReactSetupFilesAsync(outputDirectory, rootNamespace, tables);

            _output.Info("  ✓ React setup files generated!");
            _output.BlankLine();

            _output.Info($"✓ Complete project generated successfully!");
            _output.Info($"  Project: {rootNamespace}");
            _output.Info($"  Tables: {tables.Count}");
            _output.Info($"  Backend: {outputDirectory}/src");
            _output.Info($"  Frontend: {outputDirectory}/client");
            _output.Info($"  Location: {outputDirectory}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate complete project");
            _output.Error($"Generation failed: {ex.Message}");
            throw;
        }
    }

    private async Task GenerateSolutionStructureAsync(ProjectGenerationOptions options)
    {
        // Get project structure first
        var projects = _structureGenerator.GetProjectsForArchitecture(options);

        // Solution file with projects
        var solutionInfo = new SolutionInfo
        {
            Name = options.ProjectName,
            OutputDirectory = options.OutputDirectory,
            Projects = projects.ToList()
        };

        var solutionContent = _solutionGenerator.Generate(solutionInfo);
        var solutionPath = Path.Combine(options.OutputDirectory, $"{options.ProjectName}.sln");
        await File.WriteAllTextAsync(solutionPath, solutionContent);

        _output.Info($"  ✓ {Path.GetFileName(solutionPath)}");

        // Create directories
        var directories = await _structureGenerator.CreateFolderStructureAsync(options);
        _output.Info($"  ✓ Created {directories.Count} directories");

        // Project files (.csproj)
        foreach (var projectInfo in projects)
        {
            var projectContent = _projectFileGenerator.Generate(projectInfo, options);
            var projectPath = Path.Combine(options.OutputDirectory, projectInfo.CsprojPath);
            
            var projectDir = Path.GetDirectoryName(projectPath);
            if (!string.IsNullOrEmpty(projectDir) && !Directory.Exists(projectDir))
            {
                Directory.CreateDirectory(projectDir);
            }
            
            await File.WriteAllTextAsync(projectPath, projectContent);
            _output.Info($"  ✓ {projectInfo.Name}.csproj");
        }
    }

    private async Task SaveFileAsync(string filePath, string content)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content);
    }

    private async Task<int> GenerateForTableAsync(
        Table table,
        DatabaseSchema schema,
        string outputDirectory,
        string rootNamespace)
    {
        int filesCount = 0;

        // 1. Entity
        var entityGen = new EntityGenerator(_loggerFactory.CreateLogger<EntityGenerator>());
        var entityCode = await entityGen.GenerateAsync(table, schema, $"{rootNamespace}.Domain.Entities");
        var entityPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Domain", "Entities", $"{table.Name}.cs");
        await SaveFileAsync(entityPath, entityCode);
        filesCount++;

        // 2. SQL Stored Procedures - SKIP HERE, will be generated once for entire schema after all tables

        // 3. Repository Interface (with schema for Master-Detail methods)
        var repoInterfaceGen = new RepositoryInterfaceGenerator(_loggerFactory.CreateLogger<RepositoryInterfaceGenerator>());
        var repoInterface = await repoInterfaceGen.GenerateAsync(table, schema, rootNamespace);
        var repoInterfacePath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Domain", "Interfaces", $"I{table.Name}Repository.cs");
        await SaveFileAsync(repoInterfacePath, repoInterface);
        filesCount++;

        // 4. Repository Implementation (with schema for Master-Detail methods)
        var repoGen = new RepositoryGenerator(_loggerFactory.CreateLogger<RepositoryGenerator>());
        var repoImpl = await repoGen.GenerateAsync(table, schema, rootNamespace);
        var repoImplPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Infrastructure", "Repositories", $"{table.Name}Repository.cs");
        await SaveFileAsync(repoImplPath, repoImpl);
        filesCount++;

        // 5. API Controller
        var apiGen = new ApiControllerGenerator(_loggerFactory.CreateLogger<ApiControllerGenerator>());
        var apiConfig = new ApiGeneratorConfig
        {
            Namespace = $"{rootNamespace}.API",
            GenerateXmlDocumentation = true,
            GenerateSwaggerAttributes = true
        };
        var apiCode = await apiGen.GenerateAsync(table, schema, apiConfig);
        var className = BaseApiGenerator.GetClassName(table.Name);
        var controllerName = $"{CodeGenerationHelpers.MakePlural(className)}Controller";
        var apiPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Controllers", $"{controllerName}.cs");
        await SaveFileAsync(apiPath, apiCode);
        filesCount++;

        // 6. React Frontend Files
        var uiConfig = new UIGeneratorConfig
        {
            OutputDirectory = Path.Combine(outputDirectory, "client"),
            ApiBaseUrl = "http://localhost:5000",
            Framework = UIFramework.MaterialUI
        };

        // TypeScript Types
        var typeGen = new TypeScriptTypeGenerator(_loggerFactory.CreateLogger<TypeScriptTypeGenerator>());
        var typeCode = await typeGen.GenerateAsync(table, schema, uiConfig);
        var typePath = Path.Combine(outputDirectory, "client", "src", "types", $"{className}.types.ts");
        await SaveFileAsync(typePath, typeCode);
        filesCount++;

        // React API Client
        var reactApiGen = new ReactApiGenerator(_loggerFactory.CreateLogger<ReactApiGenerator>());
        var reactApiCode = await reactApiGen.GenerateAsync(table, schema, uiConfig);
        var camelClassName = char.ToLowerInvariant(className[0]) + className.Substring(1);
        var reactApiPath = Path.Combine(outputDirectory, "client", "src", "api", $"{camelClassName}Api.ts");
        await SaveFileAsync(reactApiPath, reactApiCode);
        filesCount++;

        // React Hooks
        var hookGen = new ReactHookGenerator(_loggerFactory.CreateLogger<ReactHookGenerator>());
        var hookCode = await hookGen.GenerateAsync(table, schema, uiConfig);
        var hookPath = Path.Combine(outputDirectory, "client", "src", "hooks", $"use{className}.ts");
        await SaveFileAsync(hookPath, hookCode);
        filesCount++;

        // React Components
        var componentConfig = new ComponentGeneratorConfig
        {
            OutputDirectory = Path.Combine(outputDirectory, "client", "src", "components"),
        };

        var listGen = new ReactListComponentGenerator(_loggerFactory.CreateLogger<ReactListComponentGenerator>());
        var listCode = await listGen.GenerateAsync(table, schema, componentConfig);
        var listPath = Path.Combine(outputDirectory, "client", "src", "components", className, $"{className}List.tsx");
        await SaveFileAsync(listPath, listCode);
        filesCount++;

        // Only generate Form for tables, not for views (views are read-only)
        if (!table.IsView)
        {
            var formGen = new ReactFormComponentGenerator(_loggerFactory.CreateLogger<ReactFormComponentGenerator>());
            var formCode = await formGen.GenerateAsync(table, schema, componentConfig);
            var formPath = Path.Combine(outputDirectory, "client", "src", "components", className, $"{className}Form.tsx");
            await SaveFileAsync(formPath, formCode);
            filesCount++;
        }

        var detailGen = new ReactDetailComponentGenerator(_loggerFactory.CreateLogger<ReactDetailComponentGenerator>());
        var detailCode = await detailGen.GenerateAsync(table, schema, componentConfig);
        var detailPath = Path.Combine(outputDirectory, "client", "src", "components", className, $"{className}Detail.tsx");
        await SaveFileAsync(detailPath, detailCode);
        filesCount++;

        return filesCount;
    }

    private async Task GenerateProgramCsAsync(
        string outputDirectory,
        string rootNamespace,
        string connectionString,
        List<Table> tables)
    {
        var projectInfo = new ProjectInfo
        {
            Name = $"{rootNamespace}.API",
            Type = ProjectType.Api,
            RelativePath = $"src/{rootNamespace}.API",
            Namespace = rootNamespace,
            ConnectionString = connectionString,
            Tables = tables
        };

        var content = _programCsGenerator.Generate(projectInfo);
        var path = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "Program.cs");
        await SaveFileAsync(path, content);
        _output.Info("  ✓ Program.cs");
    }

    private async Task GenerateAppSettingsAsync(
        string outputDirectory,
        string rootNamespace,
        string connectionString)
    {
        var projectInfo = new ProjectInfo
        {
            Name = $"{rootNamespace}.API",
            Type = ProjectType.Api,
            RelativePath = $"src/{rootNamespace}.API",
            Namespace = rootNamespace,
            ConnectionString = connectionString
        };

        var content = _appSettingsGenerator.Generate(projectInfo);
        var path = Path.Combine(outputDirectory, "src", $"{rootNamespace}.API", "appsettings.json");
        await SaveFileAsync(path, content);
        _output.Info("  ✓ appsettings.json");
    }

    private async Task GenerateDependencyInjectionAsync(
        string outputDirectory,
        string rootNamespace,
        List<Table> tables)
    {
        var projectInfo = new ProjectInfo
        {
            Name = $"{rootNamespace}.Application",
            Type = ProjectType.Application,
            RelativePath = $"src/{rootNamespace}.Application",
            Namespace = rootNamespace,
            Tables = tables
        };

        // Application DI
        var applicationDI = _diGenerator.GenerateApplicationDI(projectInfo);
        var appDIPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Application", "DependencyInjection.cs");
        await SaveFileAsync(appDIPath, applicationDI);
        _output.Info("  ✓ Application/DependencyInjection.cs");

        // Infrastructure DI
        projectInfo.Name = $"{rootNamespace}.Infrastructure";
        projectInfo.Type = ProjectType.Infrastructure;
        projectInfo.RelativePath = $"src/{rootNamespace}.Infrastructure";

        var infrastructureDI = _diGenerator.GenerateInfrastructureDI(projectInfo);
        var infraDIPath = Path.Combine(outputDirectory, "src", $"{rootNamespace}.Infrastructure", "DependencyInjection.cs");
        await SaveFileAsync(infraDIPath, infrastructureDI);
        _output.Info("  ✓ Infrastructure/DependencyInjection.cs");
    }

    private async Task GenerateReactSetupFilesAsync(
        string outputDirectory,
        string rootNamespace,
        List<Table> tables)
    {
        var clientDir = Path.Combine(outputDirectory, "client");

        // package.json
        var packageJson = GeneratePackageJson(rootNamespace);
        await SaveFileAsync(Path.Combine(clientDir, "package.json"), packageJson);
        _output.Info("  ✓ package.json");

        // tsconfig.json
        var tsConfig = GenerateTsConfig();
        await SaveFileAsync(Path.Combine(clientDir, "tsconfig.json"), tsConfig);
        _output.Info("  ✓ tsconfig.json");

        // tsconfig.node.json
        var tsConfigNode = GenerateTsConfigNode();
        await SaveFileAsync(Path.Combine(clientDir, "tsconfig.node.json"), tsConfigNode);
        _output.Info("  ✓ tsconfig.node.json");

        // vite.config.ts
        var viteConfig = GenerateViteConfig();
        await SaveFileAsync(Path.Combine(clientDir, "vite.config.ts"), viteConfig);
        _output.Info("  ✓ vite.config.ts");

        // index.html (root of client folder for Vite)
        var indexHtml = GenerateIndexHtml(rootNamespace);
        await SaveFileAsync(Path.Combine(clientDir, "index.html"), indexHtml);
        _output.Info("  ✓ index.html");

        // src/index.tsx
        var indexTsx = GenerateIndexTsx();
        await SaveFileAsync(Path.Combine(clientDir, "src", "index.tsx"), indexTsx);
        _output.Info("  ✓ src/index.tsx");

        // src/App.tsx
        var appTsx = GenerateAppTsx(tables);
        await SaveFileAsync(Path.Combine(clientDir, "src", "App.tsx"), appTsx);
        _output.Info("  ✓ src/App.tsx");

        // src/api/client.ts (axios setup)
        var apiClient = GenerateApiClient();
        await SaveFileAsync(Path.Combine(clientDir, "src", "api", "client.ts"), apiClient);
        _output.Info("  ✓ src/api/client.ts");

        // .gitignore
        var gitignore = GenerateGitignore();
        await SaveFileAsync(Path.Combine(clientDir, ".gitignore"), gitignore);
        _output.Info("  ✓ .gitignore");

        // README.md
        var readme = GenerateReactReadme(rootNamespace);
        await SaveFileAsync(Path.Combine(clientDir, "README.md"), readme);
        _output.Info("  ✓ README.md");
    }

    private static string GeneratePackageJson(string projectName)
    {
        return $$"""
        {
          "name": "{{projectName.ToLowerInvariant()}}-client",
          "version": "0.1.0",
          "private": true,
          "dependencies": {
            "@emotion/react": "^11.14.0",
            "@emotion/styled": "^11.14.0",
            "@mui/icons-material": "^6.3.0",
            "@mui/material": "^6.3.0",
            "@mui/x-data-grid": "^7.24.0",
            "@tanstack/react-query": "^5.62.0",
            "axios": "^1.7.9",
            "react": "^18.3.1",
            "react-dom": "^18.3.1",
            "react-hook-form": "^7.54.0",
            "react-router-dom": "^7.1.1",
            "xlsx": "^0.18.5"
          },
          "devDependencies": {
            "@types/node": "^22.10.2",
            "@types/react": "^18.3.18",
            "@types/react-dom": "^18.3.5",
            "@vitejs/plugin-react": "^4.3.4",
            "typescript": "^5.7.2",
            "vite": "^6.2.6"
          },
          "scripts": {
            "dev": "vite",
            "build": "tsc && vite build",
            "preview": "vite preview",
            "lint": "tsc --noEmit"
          }
        }
        """;
    }

    private static string GenerateTsConfig()
    {
        return """
        {
          "compilerOptions": {
            "target": "ES2020",
            "useDefineForClassFields": true,
            "lib": ["ES2020", "DOM", "DOM.Iterable"],
            "module": "ESNext",
            "skipLibCheck": true,
            "moduleResolution": "bundler",
            "allowImportingTsExtensions": true,
            "resolveJsonModule": true,
            "isolatedModules": true,
            "noEmit": true,
            "jsx": "react-jsx",
            "strict": true,
            "noUnusedLocals": true,
            "noUnusedParameters": true,
            "noFallthroughCasesInSwitch": true
          },
          "include": ["src"],
          "references": [{ "path": "./tsconfig.node.json" }]
        }
        """;
    }

    private static string GenerateTsConfigNode()
    {
        return """
        {
          "compilerOptions": {
            "composite": true,
            "skipLibCheck": true,
            "module": "ESNext",
            "moduleResolution": "bundler",
            "allowSyntheticDefaultImports": true
          },
          "include": ["vite.config.ts"]
        }
        """;
    }

    private static string GenerateViteConfig()
    {
        return """
        import { defineConfig } from 'vite'
        import react from '@vitejs/plugin-react'

        // https://vitejs.dev/config/
        export default defineConfig({
          plugins: [react()],
          server: {
            port: 5173,
            proxy: {
              '/api': {
                target: 'http://localhost:5000',
                changeOrigin: true,
              },
            },
          },
        })
        """;
    }

    private static string GenerateIndexHtml(string projectName)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="en">
          <head>
            <meta charset="UTF-8" />
            <link rel="icon" type="image/svg+xml" href="/vite.svg" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>{{projectName}}</title>
          </head>
          <body>
            <div id="root"></div>
            <script type="module" src="/src/index.tsx"></script>
          </body>
        </html>
        """;
    }

    private static string GenerateIndexTsx()
    {
        return """
        import React from 'react';
        import ReactDOM from 'react-dom/client';
        import { BrowserRouter } from 'react-router-dom';
        import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
        import App from './App';

        const queryClient = new QueryClient({
          defaultOptions: {
            queries: {
              refetchOnWindowFocus: false,
              retry: 1,
            },
          },
        });

        ReactDOM.createRoot(document.getElementById('root')!).render(
          <React.StrictMode>
            <QueryClientProvider client={queryClient}>
              <BrowserRouter
                future={{
                  v7_startTransition: true,
                  v7_relativeSplatPath: true,
                }}
              >
                <App />
              </BrowserRouter>
            </QueryClientProvider>
          </React.StrictMode>
        );
        """;
    }

    private static string GenerateAppTsx(List<Table> tables)
    {
        var imports = string.Join("\n", tables.Select(t =>
        {
            var className = BaseApiGenerator.GetClassName(t.Name);
            // For views, skip Form import (views are read-only)
            if (t.IsView)
            {
                return $"import {{ {className}List }} from './components/{className}/{className}List';\nimport {{ {className}Detail }} from './components/{className}/{className}Detail';";
            }
            return $"import {{ {className}List }} from './components/{className}/{className}List';\nimport {{ {className}Detail }} from './components/{className}/{className}Detail';\nimport {{ {className}Form }} from './components/{className}/{className}Form';";
        }));

        var menuItems = string.Join("\n", tables.Select(t =>
        {
            var className = BaseApiGenerator.GetClassName(t.Name);
            var camelName = char.ToLowerInvariant(className[0]) + className.Substring(1);
            // Add "Report" suffix for views to distinguish from tables
            var displayName = t.IsView ? $"{className}s Report" : $"{className}s";
            return $"            <ListItem disablePadding>\n              <ListItemButton component={{Link}} to=\"/{camelName}s\">\n                <ListItemText primary=\"{displayName}\" />\n              </ListItemButton>\n            </ListItem>";
        }));

        var routes = string.Join("\n", tables.Select(t =>
        {
            var className = BaseApiGenerator.GetClassName(t.Name);
            var camelName = char.ToLowerInvariant(className[0]) + className.Substring(1);
            // For views, only generate List and Detail routes (no create/edit)
            if (t.IsView)
            {
                return $"            <Route path=\"/{camelName}s\" element={{<{className}List />}} />\n            <Route path=\"/{camelName}s/:id\" element={{<{className}Detail />}} />";
            }
            return $"            <Route path=\"/{camelName}s\" element={{<{className}List />}} />\n            <Route path=\"/{camelName}s/new\" element={{<{className}Form />}} />\n            <Route path=\"/{camelName}s/:id\" element={{<{className}Detail />}} />\n            <Route path=\"/{camelName}s/:id/edit\" element={{<{className}Form />}} />";
        }));

        var appName = tables.Any() ? BaseApiGenerator.GetClassName(tables[0].Name) : "App";

        return $@"import React from 'react';
import {{ Routes, Route, Link }} from 'react-router-dom';
import {{ AppBar, Toolbar, Typography, Container, Box, Drawer, List, ListItem, ListItemButton, ListItemText }} from '@mui/material';
{imports}

const drawerWidth = 240;

function App() {{
  return (
    <Box sx={{{{ display: 'flex' }}}}>
      <AppBar position=""fixed"" sx={{{{ zIndex: (theme) => theme.zIndex.drawer + 1 }}}}>
        <Toolbar>
          <Typography variant=""h6"" noWrap component=""div"">
            {appName} Admin
          </Typography>
        </Toolbar>
      </AppBar>
      <Drawer
        variant=""permanent""
        sx={{{{
          width: drawerWidth,
          flexShrink: 0,
          '& .MuiDrawer-paper': {{ width: drawerWidth, boxSizing: 'border-box' }},
        }}}}
      >
        <Toolbar />
        <Box sx={{{{ overflow: 'auto' }}}}>
          <List>
{menuItems}
          </List>
        </Box>
      </Drawer>
      <Box component=""main"" sx={{{{ flexGrow: 1, p: 3 }}}}>
        <Toolbar />
        <Container maxWidth=""xl"">
          <Routes>
            <Route path=""/"" element={{<Typography variant=""h4"">Welcome</Typography>}} />
{routes}
          </Routes>
        </Container>
      </Box>
    </Box>
  );
}}

export default App;
";
    }

    private static string GenerateApiClient()
    {
        return """
        import axios from 'axios';

        const apiClient = axios.create({
          baseURL: 'http://localhost:5000/api',
          headers: {
            'Content-Type': 'application/json',
          },
        });

        // Request interceptor
        apiClient.interceptors.request.use(
          (config) => {
            // Add auth token if available
            const token = localStorage.getItem('token');
            if (token) {
              config.headers.Authorization = `Bearer ${token}`;
            }
            return config;
          },
          (error) => Promise.reject(error)
        );

        // Response interceptor
        apiClient.interceptors.response.use(
          (response) => response,
          (error) => {
            if (error.response?.status === 401) {
              // Handle unauthorized
              localStorage.removeItem('token');
              window.location.href = '/login';
            }
            return Promise.reject(error);
          }
        );

        export const api = apiClient;
        export default apiClient;
        """;
    }

    private static string GenerateGitignore()
    {
        return """
        # Dependencies
        node_modules
        /.pnp
        .pnp.js

        # Testing
        /coverage

        # Production
        /build
        /dist

        # Misc
        .DS_Store
        .env.local
        .env.development.local
        .env.test.local
        .env.production.local

        npm-debug.log*
        yarn-debug.log*
        yarn-error.log*

        # Editor
        .vscode
        .idea
        """;
    }

    private static string GenerateReactReadme(string projectName)
    {
        return $$"""
        # {{projectName}} - React Frontend

        This is the React frontend for {{projectName}}, built with:
        - React 18
        - TypeScript
        - Material-UI
        - React Router
        - React Query
        - Vite

        ## Getting Started

        1. Install dependencies:
        ```bash
        npm install
        ```

        2. Start the development server:
        ```bash
        npm run dev
        ```

        3. Open http://localhost:5173 in your browser

        ## Available Scripts

        - `npm run dev` - Start development server
        - `npm run build` - Build for production
        - `npm run preview` - Preview production build
        - `npm run lint` - Run TypeScript checks

        ## Project Structure

        ```
        src/
          api/         - API client setup and endpoints
          components/  - React components
          hooks/       - Custom React hooks
          types/       - TypeScript type definitions
          App.tsx      - Main app component
          index.tsx    - Entry point
        ```
        """;
    }
}
