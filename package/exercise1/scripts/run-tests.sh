#!/bin/sh
# =============================================================================
# run-tests.sh — Run the Stargate ACTS test suite
# =============================================================================
# Run this script from the exercise1/ root to execute all unit tests
# and generate a code coverage report.
#
# Prerequisites:
#   - .NET 8 SDK installed
#   - tests/StargateAPI.Tests project exists
#
# Usage:
#   ./scripts/run-tests.sh
# =============================================================================

set -e

echo "=== Running Stargate Test Suite ==="

cd tests/StargateAPI.Tests

echo "1. Restoring test dependencies..."
dotnet restore

echo "2. Running tests with coverage..."
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

echo "=== Test run complete. Coverage report: tests/StargateAPI.Tests/coverage/ ==="
