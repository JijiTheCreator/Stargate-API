#!/bin/sh
set -e

echo "=== Stargate API Starting ==="
echo "Applying database migrations..."

# Run the application (EF Core will apply migrations on startup via Program.cs)
exec dotnet StargateAPI.dll
