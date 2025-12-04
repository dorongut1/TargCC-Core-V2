#!/bin/bash

# Script to fix all generator namespace issues

echo "Fixing RepositoryInterfaceGenerator..."
sed -i 's/public async Task<string> GenerateAsync(Table table)/public async Task<string> GenerateAsync(Table table, string rootNamespace = "YourApp")/g' \
    src/TargCC.Core.Generators/Repositories/RepositoryInterfaceGenerator.cs

sed -i 's/GenerateUsings(sb);/GenerateUsings(sb, rootNamespace);/g' \
    src/TargCC.Core.Generators/Repositories/RepositoryInterfaceGenerator.cs

sed -i 's/private static void GenerateUsings(StringBuilder sb)/private static void GenerateUsings(StringBuilder sb, string rootNamespace)/g' \
    src/TargCC.Core.Generators/Repositories/RepositoryInterfaceGenerator.cs

sed -i 's/sb.AppendLine("namespace TargCC.Domain.Interfaces;");/sb.AppendLine($"namespace {rootNamespace}.Domain.Interfaces;");/g' \
    src/TargCC.Core.Generators/Repositories/RepositoryInterfaceGenerator.cs

sed -i 's/sb.AppendLine("using TargCC.Domain.Entities;");/sb.AppendLine($"using {rootNamespace}.Domain.Entities;");/g' \
    src/TargCC.Core.Generators/Repositories/RepositoryInterfaceGenerator.cs

echo "Fixing RepositoryGenerator..."
# Similar fixes for RepositoryGenerator

echo "Fixing ProjectGenerationService to pass rootNamespace..."
sed -i 's/await repoInterfaceGen.GenerateAsync(table);/await repoInterfaceGen.GenerateAsync(table, rootNamespace);/g' \
    src/TargCC.CLI/Services/Generation/ProjectGenerationService.cs

echo "Done! All generators fixed."
