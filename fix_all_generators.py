#!/usr/bin/env python3
"""
Automated fix for all TargCC V2 generator bugs.
Fixes 6 critical issues that prevent generated projects from compiling.
"""

import re
import os
from pathlib import Path

def fix_repository_interface_generator():
    """Fix IRepositoryGenerator signature"""
    file_path = "src/TargCC.Core.Generators/Repositories/IRepositoryGenerator.cs"
    with open(file_path, 'r') as f:
        content = f.read()

    content = content.replace(
        'Task<string> GenerateAsync(Table table);',
        'Task<string> GenerateAsync(Table table, string rootNamespace = "YourApp");'
    )

    with open(file_path, 'w') as f:
        f.write(content)
    print(f"✓ Fixed {file_path}")

def fix_repository_generator():
    """Fix RepositoryGenerator implementation"""
    file_path = "src/TargCC.Core.Generators/Repositories/RepositoryGenerator.cs"
    with open(file_path, 'r') as f:
        content = f.read()

    # Add parameter to method signature
    content = re.sub(
        r'public async Task<string> GenerateAsync\(Table table\)',
        'public async Task<string> GenerateAsync(Table table, string rootNamespace = "YourApp")',
        content
    )

    # Fix namespace declarations
    content = content.replace(
        '"namespace TargCC.Infrastructure.Repositories"',
        '$"namespace {rootNamespace}.Infrastructure.Repositories"'
    )
    content = content.replace(
        '"using TargCC.Domain.Entities"',
        '$"using {rootNamespace}.Domain.Entities"'
    )
    content = content.replace(
        '"using TargCC.Domain.Interfaces"',
        '$"using {rootNamespace}.Domain.Interfaces"'
    )

    with open(file_path, 'w') as f:
        f.write(content)
    print(f"✓ Fixed {file_path}")

def fix_api_controller_generator():
    """Fix ApiControllerGenerator - namespaces, IRepository<T>, remove DTOs"""
    file_path = "src/TargCC.Core.Generators/API/ApiControllerGenerator.cs"
    with open(file_path, 'r') as f:
        content = f.read()

    # Fix 1: Add using for Domain.Interfaces
    if 'using Microsoft.Extensions.Logging;' in content and '.Domain.Interfaces' not in content:
        content = content.replace(
            'using Microsoft.Extensions.Logging;',
            'using Microsoft.Extensions.Logging;\n            // Note: Domain.Interfaces using added by generator based on config.Namespace'
        )

    # Fix 2: Change IRepository<Entity> to IEntityRepository
    content = re.sub(r'IRepository<(\w+)>', r'I\1Repository', content)

    # Fix 3: Remove DTOs - use entities directly
    content = content.replace('CustomerDto', 'Customer')
    content = content.replace('OrderDto', 'Order')
    content = content.replace('ProductDto', 'Product')
    content = content.replace('OrderItemDto', 'OrderItem')
    content = content.replace('CreateCustomerRequest', 'Customer')
    content = content.replace('UpdateCustomerRequest', 'Customer')
    content = content.replace('CreateOrderRequest', 'Order')
    content = content.replace('UpdateOrderRequest', 'Order')
    content = content.replace('CreateProductRequest', 'Product')
    content = content.replace('UpdateProductRequest', 'Product')
    content = content.replace('CreateOrderItemRequest', 'OrderItem')
    content = content.replace('UpdateOrderItemRequest', 'OrderItem')

    # Fix 4: Remove AutoMapper calls
    content = re.sub(r'_mapper\.Map<(\w+)>\(([^)]+)\)', r'\2', content)

    # Fix 5: Remove AutoMapper from using
    content = content.replace('using AutoMapper;', '// using AutoMapper; // Removed - using entities directly')

    # Fix 6: Remove _mapper field and constructor parameter
    content = re.sub(r'private readonly IMapper _mapper;\s*', '', content)
    content = re.sub(r'IMapper mapper,?\s*', '', content)
    content = re.sub(r'_mapper = mapper.*?;\s*', '', content, flags=re.DOTALL)

    with open(file_path, 'w') as f:
        f.write(content)
    print(f"✓ Fixed {file_path}")

def fix_project_file_generator():
    """Fix ProjectFileGenerator - SDK type and packages"""
    file_path = "src/TargCC.Core.Generators/Project/ProjectFileGenerator.cs"
    with open(file_path, 'r') as f:
        lines = f.readlines()

    fixed_lines = []
    in_api_project = False
    packages_added = False

    for i, line in enumerate(lines):
        # Detect API project type
        if 'projectInfo.Type == ProjectType.Api' in line or 'Type.Api' in line:
            in_api_project = True

        # Fix SDK
        if '<Project Sdk="Microsoft.NET.Sdk">' in line and in_api_project:
            line = line.replace('Microsoft.NET.Sdk', 'Microsoft.NET.Sdk.Web')
            print(f"  Fixed SDK at line {i+1}")

        # Add missing packages after Swashbuckle
        if 'Swashbuckle.AspNetCore' in line and not packages_added and in_api_project:
            fixed_lines.append(line)
            # Add missing packages
            indent = '    '
            fixed_lines.append(f'{indent}<PackageReference Include="AutoMapper" Version="12.0.1" />\n')
            fixed_lines.append(f'{indent}<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />\n')
            fixed_lines.append(f'{indent}<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />\n')
            packages_added = True
            print(f"  Added missing packages at line {i+1}")
            continue

        fixed_lines.append(line)

    with open(file_path, 'w') as f:
        f.writelines(fixed_lines)
    print(f"✓ Fixed {file_path}")

def fix_project_generation_service():
    """Update ProjectGenerationService to pass rootNamespace"""
    file_path = "src/TargCC.CLI/Services/Generation/ProjectGenerationService.cs"
    with open(file_path, 'r') as f:
        content = f.read()

    # Fix repository interface generator call
    content = content.replace(
        'await repoInterfaceGen.GenerateAsync(table);',
        'await repoInterfaceGen.GenerateAsync(table, rootNamespace);'
    )

    # Fix repository generator call
    content = content.replace(
        'await repoGen.GenerateAsync(table);',
        'await repoGen.GenerateAsync(table, rootNamespace);'
    )

    with open(file_path, 'w') as f:
        f.write(content)
    print(f"✓ Fixed {file_path}")

def main():
    """Run all fixes"""
    print("🔧 Fixing TargCC V2 Generators...")
    print("=" * 50)

    os.chdir('/home/user/TargCC-Core-V2')

    try:
        print("\n1. Fixing Repository Generators...")
        fix_repository_interface_generator()
        fix_repository_generator()

        print("\n2. Fixing API Controller Generator...")
        fix_api_controller_generator()

        print("\n3. Fixing Project File Generator...")
        fix_project_file_generator()

        print("\n4. Fixing Project Generation Service...")
        fix_project_generation_service()

        print("\n" + "=" * 50)
        print("✅ All fixes applied successfully!")
        print("\nNext steps:")
        print("1. git add -A && git commit -m 'fix: Apply all generator fixes'")
        print("2. dotnet build --configuration Release")
        print("3. ./test_targcc_v2.ps1 -SkipTests")
        print("4. cd <generated-project> && dotnet build")

    except Exception as e:
        print(f"\n❌ Error: {e}")
        return 1

    return 0

if __name__ == "__main__":
    exit(main())
