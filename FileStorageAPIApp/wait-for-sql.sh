#!/bin/bash
set -e

echo "Waiting for SQL Server to be ready..."
until /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "AdminPas@1234#" -Q "SELECT 1" &> /dev/null
do
  echo "SQL Server is unavailable - sleeping"
  sleep 2
done

echo "SQL Server is up - executing migrations"
cd /src

# Run database migrations
dotnet ef database update --project FileStorage.Infrastructure/FileStorage.Infrastructure.csproj --startup-project FileStorage.API/FileStorage.API.csproj

echo "Migrations completed successfully"

