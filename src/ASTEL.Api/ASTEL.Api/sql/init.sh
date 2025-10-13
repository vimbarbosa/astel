#!/bin/bash
set -e

echo "Starting SQL Server..."
/opt/mssql/bin/sqlservr &

echo "Waiting for SQL Server to become available..."
for i in {1..60}; do
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &>/dev/null && break
  echo "  ...still waiting ($i)"
  sleep 2
done

echo "Running /sql/tables.sql..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -i /sql/tables.sql

echo "Initialization complete. Keeping SQL Server running..."
wait
