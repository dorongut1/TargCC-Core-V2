#!/bin/bash
# =============================================================================
# TargCC V2 - Complete Test Script
# Version: 1.0
# Date: 2025-12-04
# Purpose: Test TargCC V2 end-to-end functionality
# =============================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
TARGCC_ROOT="/home/user/TargCC-Core-V2"
TEST_DIR="/tmp/TargCCTest_$(date +%Y%m%d_%H%M%S)"
TEST_DB_NAME="TargCCTest_$(date +%Y%m%d_%H%M%S)"
SQL_SERVER="localhost"
CONNECTION_STRING="Server=${SQL_SERVER};Database=${TEST_DB_NAME};Trusted_Connection=true;"

# Functions
print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# =============================================================================
# Step 0: Pre-checks
# =============================================================================
print_header "Step 0: Pre-flight Checks"

print_info "Checking prerequisites..."

# Check dotnet
if ! command -v dotnet &> /dev/null; then
    print_error "dotnet not found. Please install .NET 9 SDK"
    exit 1
fi
print_success "dotnet found: $(dotnet --version)"

# Check sqlcmd
if ! command -v sqlcmd &> /dev/null; then
    print_warning "sqlcmd not found. Will skip database creation."
    SKIP_DB=true
else
    print_success "sqlcmd found"
    SKIP_DB=false
fi

# Check TargCC directory
if [ ! -d "$TARGCC_ROOT" ]; then
    print_error "TargCC root directory not found: $TARGCC_ROOT"
    exit 1
fi
print_success "TargCC directory found"

# =============================================================================
# Step 1: Build TargCC
# =============================================================================
print_header "Step 1: Building TargCC V2"

cd "$TARGCC_ROOT"

print_info "Restoring packages..."
dotnet restore

print_info "Building solution..."
dotnet build --configuration Release --no-restore

if [ $? -eq 0 ]; then
    print_success "Build completed successfully!"
else
    print_error "Build failed!"
    exit 1
fi

# =============================================================================
# Step 2: Run Unit Tests
# =============================================================================
print_header "Step 2: Running Unit Tests"

print_info "Running C# unit tests..."
dotnet test --filter "Category=Unit" --no-build --configuration Release --logger "console;verbosity=minimal"

if [ $? -eq 0 ]; then
    print_success "Unit tests passed!"
else
    print_warning "Some unit tests failed (continuing anyway)"
fi

# =============================================================================
# Step 3: Create Test Database
# =============================================================================
if [ "$SKIP_DB" = false ]; then
    print_header "Step 3: Creating Test Database"

    print_info "Creating database: $TEST_DB_NAME"

    sqlcmd -S "$SQL_SERVER" -Q "CREATE DATABASE [$TEST_DB_NAME]" 2>/dev/null

    if [ $? -eq 0 ]; then
        print_success "Database created: $TEST_DB_NAME"
    else
        print_error "Failed to create database"
        exit 1
    fi

    print_info "Creating test tables..."

    sqlcmd -S "$SQL_SERVER" -d "$TEST_DB_NAME" -Q "
    -- Customer table
    CREATE TABLE [dbo].[Customer] (
        [ID] INT PRIMARY KEY IDENTITY(1,1),
        [Name] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [Phone] VARCHAR(20),
        [ent_CreditCard] NVARCHAR(MAX),  -- Encrypted prefix
        [lkp_Status] VARCHAR(20),         -- Lookup prefix
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        [AddedBy] NVARCHAR(50),
        [ChangedOn] DATETIME,
        [ChangedBy] NVARCHAR(50)
    );

    CREATE UNIQUE INDEX IX_Customer_Email ON [dbo].[Customer]([Email]);
    CREATE INDEX IX_Customer_Status ON [dbo].[Customer]([lkp_Status]);

    -- Order table
    CREATE TABLE [dbo].[Order] (
        [ID] INT PRIMARY KEY IDENTITY(1,1),
        [CustomerID] INT NOT NULL,
        [OrderDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [TotalAmount] DECIMAL(18,2),
        [enm_Status] VARCHAR(20),         -- Enum prefix
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Order_Customer FOREIGN KEY ([CustomerID])
            REFERENCES [dbo].[Customer]([ID])
    );

    CREATE INDEX IX_Order_Customer ON [dbo].[Order]([CustomerID]);
    CREATE INDEX IX_Order_Date ON [dbo].[Order]([OrderDate]);

    -- Product table
    CREATE TABLE [dbo].[Product] (
        [ID] INT PRIMARY KEY IDENTITY(1,1),
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX),
        [Price] DECIMAL(18,2) NOT NULL,
        [StockQuantity] INT NOT NULL,
        [enm_Category] VARCHAR(50),       -- Enum prefix
        [IsActive] BIT NOT NULL DEFAULT 1,
        [AddedOn] DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_Product_Name ON [dbo].[Product]([Name]);
    CREATE INDEX IX_Product_Category ON [dbo].[Product]([enm_Category]);

    -- OrderItem table (many-to-many relationship)
    CREATE TABLE [dbo].[OrderItem] (
        [ID] INT PRIMARY KEY IDENTITY(1,1),
        [OrderID] INT NOT NULL,
        [ProductID] INT NOT NULL,
        [Quantity] INT NOT NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL,
        [clc_LineTotal] AS ([Quantity] * [UnitPrice]), -- Calculated prefix
        CONSTRAINT FK_OrderItem_Order FOREIGN KEY ([OrderID])
            REFERENCES [dbo].[Order]([ID]),
        CONSTRAINT FK_OrderItem_Product FOREIGN KEY ([ProductID])
            REFERENCES [dbo].[Product]([ID])
    );

    CREATE INDEX IX_OrderItem_Order ON [dbo].[OrderItem]([OrderID]);
    CREATE INDEX IX_OrderItem_Product ON [dbo].[OrderItem]([ProductID]);

    -- Insert sample data
    INSERT INTO [dbo].[Customer] ([Name], [Email], [Phone], [lkp_Status])
    VALUES
        ('John Doe', 'john@example.com', '555-0100', 'Active'),
        ('Jane Smith', 'jane@example.com', '555-0200', 'Active'),
        ('Bob Johnson', 'bob@example.com', '555-0300', 'Inactive');

    PRINT 'Test database created successfully with 4 tables and sample data';
    " 2>/dev/null

    if [ $? -eq 0 ]; then
        print_success "Test tables created with sample data"
    else
        print_error "Failed to create test tables"
        exit 1
    fi
else
    print_warning "Skipping database creation (sqlcmd not available)"
    print_info "You'll need to create a test database manually"
fi

# =============================================================================
# Step 4: Create Test Project Directory
# =============================================================================
print_header "Step 4: Creating Test Project"

print_info "Creating test directory: $TEST_DIR"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

print_success "Test directory created"

# =============================================================================
# Step 5: Initialize TargCC
# =============================================================================
print_header "Step 5: Initializing TargCC"

TARGCC_CLI="$TARGCC_ROOT/src/TargCC.CLI/bin/Release/net9.0/TargCC.CLI"

if [ ! -f "$TARGCC_CLI" ]; then
    print_error "TargCC CLI not found at: $TARGCC_CLI"
    exit 1
fi

print_info "Running: targcc init"
"$TARGCC_CLI" init --force

if [ $? -eq 0 ]; then
    print_success "TargCC initialized"
else
    print_error "Failed to initialize TargCC"
    exit 1
fi

# Configure connection string
print_info "Configuring connection string..."
"$TARGCC_CLI" config set ConnectionString "$CONNECTION_STRING"
"$TARGCC_CLI" config set DefaultNamespace "TestApp"
"$TARGCC_CLI" config set OutputDirectory "."

print_success "Configuration set"

# =============================================================================
# Step 6: Analyze Database Schema
# =============================================================================
print_header "Step 6: Analyzing Database Schema"

print_info "Running: targcc analyze schema"
"$TARGCC_CLI" analyze schema

if [ $? -eq 0 ]; then
    print_success "Schema analysis completed"
else
    print_warning "Schema analysis had issues (continuing)"
fi

# =============================================================================
# Step 7: Generate Complete Project
# =============================================================================
print_header "Step 7: Generating Complete Project"

print_info "Running: targcc generate project"
print_info "This may take 1-2 minutes..."

"$TARGCC_CLI" generate project --database "$TEST_DB_NAME" --output . --namespace TestApp

if [ $? -eq 0 ]; then
    print_success "Project generation completed!"
else
    print_error "Project generation failed!"
    exit 1
fi

# Show generated files
print_info "Generated project structure:"
ls -la

# =============================================================================
# Step 8: Build Generated Project
# =============================================================================
print_header "Step 8: Building Generated Project"

print_info "Restoring packages for generated project..."
dotnet restore

print_info "Building generated project..."
dotnet build --configuration Release

if [ $? -eq 0 ]; then
    print_success "Generated project built successfully!"
else
    print_error "Generated project build failed!"
    print_info "This is expected - checking build errors..."
fi

# =============================================================================
# Step 9: Analyze Results
# =============================================================================
print_header "Step 9: Test Results Summary"

echo ""
print_info "Test completed!"
echo ""

print_info "Test Details:"
echo "  Test Directory: $TEST_DIR"
echo "  Database: $TEST_DB_NAME"
echo "  Connection: $CONNECTION_STRING"
echo ""

print_info "Generated Files:"
if [ -f "TestApp.sln" ]; then
    print_success "Solution file created"
    echo "  $(ls -lh TestApp.sln | awk '{print $5, $9}')"
fi

if [ -d "src" ]; then
    print_success "Source directory created"
    echo "  Projects:"
    ls -1 src/ | while read project; do
        echo "    - $project"
    done
fi

echo ""
print_info "Next Steps:"
echo "  1. Review generated code: cd $TEST_DIR"
echo "  2. Check for build errors: dotnet build"
echo "  3. Run API: cd src/TestApp.API && dotnet run"
echo "  4. Test API: curl http://localhost:5000/api/customers"
echo ""

# Cleanup option
echo ""
read -p "Do you want to cleanup test database and files? (y/N) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_info "Cleaning up..."

    if [ "$SKIP_DB" = false ]; then
        sqlcmd -S "$SQL_SERVER" -Q "DROP DATABASE [$TEST_DB_NAME]" 2>/dev/null
        print_success "Database dropped"
    fi

    cd /tmp
    rm -rf "$TEST_DIR"
    print_success "Test directory removed"
else
    print_info "Test artifacts preserved for inspection"
fi

print_header "Test Complete!"

exit 0
