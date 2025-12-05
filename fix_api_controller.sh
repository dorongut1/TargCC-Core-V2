#!/bin/bash

FILE="src/TargCC.Core.Generators/API/ApiControllerGenerator.cs"

echo "Fixing ApiControllerGenerator..."

# Fix 1: Change IRepository<T> to IEntityRepository
sed -i 's/IRepository<\([^>]*\)>/I\1Repository/g' "$FILE"

# Fix 2: Add using for Domain.Interfaces
sed -i '/using Microsoft.Extensions.Logging;/a\            sb.AppendLine($"using {config.Namespace.Replace(\".API\", \"\")}.Domain.Interfaces;");' "$FILE"

# Fix 3: Remove AutoMapper references (simplify - use entities directly)
sed -i 's/CustomerDto/Customer/g' "$FILE"
sed -i 's/OrderDto/Order/g' "$FILE"
sed -i 's/ProductDto/Product/g' "$FILE"
sed -i 's/OrderItemDto/OrderItem/g' "$FILE"

# Fix 4: Remove _mapper usage
sed -i 's/_mapper\.Map<\([^>]*\)>(\([^)]*\))/\2/g' "$FILE"

echo "Done fixing ApiControllerGenerator!"
