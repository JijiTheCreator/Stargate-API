#!/bin/sh
# =============================================================================
# init-db.sh — Initialize the Stargate ACTS database
# =============================================================================
# Run this script from the exercise1/ root to apply EF Core migrations
# and generate the SQLite database.
#
# Prerequisites:
#   - .NET 8 SDK installed
#   - dotnet-ef tool installed: dotnet tool install --global dotnet-ef
#
# Usage:
#   ./scripts/init-db.sh
# =============================================================================

set -e

echo "=== Initializing Stargate Database ==="

cd src/api

echo "1. Restoring NuGet packages..."
dotnet restore

echo "2. Applying EF Core migrations..."
dotnet ef database update

echo "=== Database initialized: src/api/starbase.db ==="
