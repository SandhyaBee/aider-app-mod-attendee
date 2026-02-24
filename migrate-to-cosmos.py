#!/usr/bin/env python3
"""Migrate denormalized product data from Azure SQL to Azure Cosmos DB."""

import json
import subprocess
import sys

from azure.cosmos import CosmosClient, PartitionKey

SQL_SERVER = "styleverse-sqlgwc-06.database.windows.net"
SQL_DATABASE = "StyleVerseDb"
SQL_ADMIN = "sqladmin"
SQL_PASSWORD = "P@ssw0rd2026!Esc"

COSMOS_ACCOUNT = "styleverse-cosmos-06"
COSMOS_DATABASE = "StyleVerseDb"
COSMOS_CONTAINER = "Products"

DENORM_QUERY = """
SET NOCOUNT ON;
SELECT p.Id, p.Name, p.Description, p.Price, p.CategoryId,
       c.Name AS CategoryName, p.InventoryCount, p.CreatedDate,
       STRING_AGG(t.Name, ',') AS Tags
FROM Products p
JOIN Categories c ON p.CategoryId = c.Id
LEFT JOIN ProductTags pt ON p.Id = pt.ProductId
LEFT JOIN Tags t ON pt.TagId = t.Id
GROUP BY p.Id, p.Name, p.Description, p.Price, p.CategoryId,
         c.Name, p.InventoryCount, p.CreatedDate
ORDER BY p.Id
FOR JSON PATH;
"""


def extract_from_sql():
    """Run the denormalized query via sqlcmd and return parsed JSON."""
    print("Extracting denormalized products from Azure SQL...")
    result = subprocess.run(
        [
            "sqlcmd",
            "-S", f"tcp:{SQL_SERVER},1433",
            "-d", SQL_DATABASE,
            "-U", SQL_ADMIN,
            "-P", SQL_PASSWORD,
            "-Q", DENORM_QUERY,
            "-C",
            "-y", "0",
            "-h", "-1",
        ],
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        print(f"sqlcmd failed:\n{result.stderr}", file=sys.stderr)
        sys.exit(1)

    raw = result.stdout.strip()
    raw = "".join(line.strip() for line in raw.splitlines() if line.strip())

    products = json.loads(raw)
    print(f"  Extracted {len(products)} products from SQL.")
    return products


def transform(products):
    """Convert SQL rows into Cosmos DB documents."""
    docs = []
    for p in products:
        doc = {
            "id": str(p["Id"]),
            "name": p["Name"],
            "description": p["Description"],
            "price": float(p["Price"]),
            "categoryId": p["CategoryId"],
            "categoryName": p["CategoryName"],
            "inventoryCount": p["InventoryCount"],
            "createdDate": p["CreatedDate"],
            "tags": [t.strip() for t in p["Tags"].split(",")] if p.get("Tags") else [],
        }
        docs.append(doc)
    return docs


def get_cosmos_key():
    """Retrieve the Cosmos DB primary key via Azure CLI."""
    print("Fetching Cosmos DB primary key...")
    result = subprocess.run(
        [
            "az", "cosmosdb", "keys", "list",
            "--name", COSMOS_ACCOUNT,
            "--resource-group", "rgDeveloperEscapeRoom-06",
            "--query", "primaryMasterKey",
            "-o", "tsv",
        ],
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        print(f"Failed to get Cosmos key:\n{result.stderr}", file=sys.stderr)
        sys.exit(1)
    return result.stdout.strip()


def load_to_cosmos(docs):
    """Upsert all documents into Cosmos DB."""
    key = get_cosmos_key()
    endpoint = f"https://{COSMOS_ACCOUNT}.documents.azure.com:443/"
    client = CosmosClient(endpoint, credential=key)
    db = client.get_database_client(COSMOS_DATABASE)
    container = db.get_container_client(COSMOS_CONTAINER)

    print(f"Upserting {len(docs)} documents into Cosmos DB...")
    for i, doc in enumerate(docs, 1):
        container.upsert_item(doc)
        if i % 10 == 0 or i == len(docs):
            print(f"  {i}/{len(docs)} documents upserted.")
    print("Migration complete!")


def main():
    products = extract_from_sql()
    docs = transform(products)
    load_to_cosmos(docs)


if __name__ == "__main__":
    main()
