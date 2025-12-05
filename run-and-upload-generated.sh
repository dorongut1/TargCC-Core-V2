#!/bin/bash

# Script to run TargCC test, find generated output, and upload to Git for review

set -e  # Exit on error

echo "========================================="
echo "Step 1: Running TargCC Test Script"
echo "========================================="
echo ""

# Run the PowerShell test script
pwsh ./test_targcc_v2.ps1

echo ""
echo "========================================="
echo "Step 2: Finding Generated Output"
echo "========================================="
echo ""

# Find the most recent TargCCTest directory in TEMP
TEMP_DIR="${TMPDIR:-/tmp}"
if [ -d "$HOME/AppData/Local/Temp" ]; then
    TEMP_DIR="$HOME/AppData/Local/Temp"
fi

GENERATED_DIR=$(find "$TEMP_DIR" -maxdepth 1 -type d -name "TargCCTest_*" 2>/dev/null | sort -r | head -n 1)

if [ -z "$GENERATED_DIR" ]; then
    echo "ERROR: Could not find generated TargCCTest directory in $TEMP_DIR"
    exit 1
fi

echo "Found generated project: $GENERATED_DIR"

echo ""
echo "========================================="
echo "Step 3: Copying Generated Files"
echo "========================================="
echo ""

# Create a directory for the generated output in the repo
OUTPUT_DIR="./generated-output"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Copy all generated files
echo "Copying files from $GENERATED_DIR to $OUTPUT_DIR..."
cp -r "$GENERATED_DIR"/* "$OUTPUT_DIR/"

echo "Files copied successfully!"

echo ""
echo "========================================="
echo "Step 4: Creating Git Branch"
echo "========================================="
echo ""

# Generate a unique branch name with required 'claude/' prefix
BRANCH_NAME="claude/review-generated-$(date +%Y%m%d-%H%M%S)"

echo "Creating new branch: $BRANCH_NAME"
git checkout -b "$BRANCH_NAME"

echo ""
echo "========================================="
echo "Step 5: Committing and Pushing"
echo "========================================="
echo ""

echo "Adding all files..."
git add -A

echo "Committing..."
git commit -m "chore: Upload generated TargCC project for review

Generated project from test_targcc_v2.ps1
Source directory: $GENERATED_DIR

This branch includes:
- Complete generated backend (API, Domain, Infrastructure, Application layers)
- Complete generated frontend (React + TypeScript)
- All configuration files

This is a temporary branch for code review to diagnose form rendering issues."

echo "Pushing to remote..."
git push -u origin "$BRANCH_NAME"

echo ""
echo "========================================="
echo "✅ COMPLETE!"
echo "========================================="
echo ""
echo "Branch created: $BRANCH_NAME"
echo "Generated files location: $OUTPUT_DIR"
echo ""
echo "Now Claude can review all the generated code!"
echo ""
echo "To delete this branch later:"
echo "  git checkout claude/review-test-script-01Te4Z88CcBDuBAs9H2PvxYk"
echo "  git branch -D $BRANCH_NAME"
echo "  git push origin --delete $BRANCH_NAME"
echo "  rm -rf $OUTPUT_DIR"
echo ""
