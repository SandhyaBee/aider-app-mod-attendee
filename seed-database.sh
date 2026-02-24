#!/bin/bash
set -e

SQL_SERVER="$1"
SQL_DB="${2:-StyleVerseDb}"
SQL_ADMIN="$3"
SQL_PASSWORD="$4"
RESOURCE_GROUP="${5:-rgDeveloperEscapeRoom-06}"
SQL_SERVER_NAME="${6}"

if [ -z "$SQL_SERVER" ] || [ -z "$SQL_ADMIN" ] || [ -z "$SQL_PASSWORD" ]; then
  echo "Usage: ./seed-database.sh <sql-server-fqdn> <database> <admin-user> <admin-password> [resource-group] [sql-server-name]"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

CLIENT_IP=$(curl -s https://api.ipify.org)
echo "Detected client IP: $CLIENT_IP"

if [ -n "$SQL_SERVER_NAME" ]; then
  echo "Adding temporary firewall rule for client IP..."
  az sql server firewall-rule create \
    --resource-group "$RESOURCE_GROUP" \
    --server "$SQL_SERVER_NAME" \
    --name "TempClientAccess" \
    --start-ip-address "$CLIENT_IP" \
    --end-ip-address "$CLIENT_IP"
fi

echo "Seeding database $SQL_DB on $SQL_SERVER..."
sqlcmd -S "tcp:${SQL_SERVER},1433" -d "$SQL_DB" -U "$SQL_ADMIN" -P "$SQL_PASSWORD" -i "$SCRIPT_DIR/database.sql" -C

echo "Database seeded successfully."

if [ -n "$SQL_SERVER_NAME" ]; then
  echo "Removing temporary firewall rule..."
  az sql server firewall-rule delete \
    --resource-group "$RESOURCE_GROUP" \
    --server "$SQL_SERVER_NAME" \
    --name "TempClientAccess" \
    --yes
  echo "Temporary firewall rule removed."
fi
