#!/bin/bash

# Configuration
CONTAINER_NAME="order-postgres"
DB_USER="postgres"
DB_NAME="OrderDb"
SCHEMA_FILE="database_schema.sql"

# Check if container is running
if ! docker ps | grep -q "$CONTAINER_NAME"; then
    echo "Error: Container '$CONTAINER_NAME' is not running."
    echo "Please run 'docker-compose up -d' first."
    exit 1
fi

# Wait for Postgres to be ready
echo "Waiting for Postgres to be ready..."
until docker exec "$CONTAINER_NAME" pg_isready -U "$DB_USER" > /dev/null 2>&1; do
    sleep 1
done

echo "Initializing database '$DB_NAME'..."

# Execute the schema script
cat "$SCHEMA_FILE" | docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" -d "$DB_NAME"

if [ $? -eq 0 ]; then
    echo "Database initialized successfully."
else
    echo "Error initializing database."
    exit 1
fi
