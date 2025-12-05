#!/bin/bash

# Script to upload all generated files for review
# This creates a temporary branch that can be deleted later

set -e  # Exit on error

# Generate a unique branch name with required 'claude/' prefix
BRANCH_NAME="claude/review-generated-code-$(date +%Y%m%d-%H%M%S)"

echo "Creating new branch: $BRANCH_NAME"
git checkout -b "$BRANCH_NAME"

echo "Adding all files (including generated ones)..."
git add -A

echo "Committing all files..."
git commit -m "chore: Upload all generated code for review

This is a temporary branch containing all auto-generated files
for code review and debugging purposes.

This branch can be safely deleted after review."

echo "Pushing to remote..."
git push -u origin "$BRANCH_NAME"

echo ""
echo "✅ Done!"
echo ""
echo "Branch created: $BRANCH_NAME"
echo "You can now review all the code."
echo ""
echo "To delete this branch later, run:"
echo "  git checkout main"
echo "  git branch -D $BRANCH_NAME"
echo "  git push origin --delete $BRANCH_NAME"
