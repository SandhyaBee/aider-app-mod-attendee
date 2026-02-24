// Migration Script: Normalized Categories and Tags to Cosmos DB
// Creates reference/lookup documents for Categories and Tags

const { CosmosClient } = require("@azure/cosmos");

const COSMOS_ENDPOINT = "https://styleverse-cosmos-06.documents.azure.com:443/";
const COSMOS_KEY = process.env.COSMOS_KEY;
const DATABASE_NAME = "StyleVerseDb";

// Normalized Categories (from database.sql)
const categories = [
  { id: "1", name: "Streetwear", description: "Urban street fashion and casual wear" },
  { id: "2", name: "Formal", description: "Professional and elegant attire" },
  { id: "3", name: "Accessories", description: "Fashion accessories and add-ons" },
  { id: "4", name: "Techwear", description: "Technical and functional clothing" },
  { id: "5", name: "Footwear", description: "Shoes, boots, and sandals" }
];

// Normalized Tags (from database.sql)
const tags = [
  { id: "1", name: "Cotton", description: "Made with cotton material" },
  { id: "2", name: "Unisex", description: "Suitable for all genders" },
  { id: "3", name: "Limited Edition", description: "Exclusive limited release" },
  { id: "4", name: "Summer", description: "Perfect for summer season" },
  { id: "5", name: "Waterproof", description: "Water-resistant material" },
  { id: "6", name: "Recycled", description: "Made from recycled materials" },
  { id: "7", name: "Oversized", description: "Loose, oversized fit" },
  { id: "8", name: "Neon", description: "Bright neon colors" },
  { id: "9", name: "Breathable", description: "Allows air circulation" },
  { id: "10", name: "Windproof", description: "Blocks wind effectively" }
];

async function migrate() {
  if (!COSMOS_KEY) {
    console.error("ERROR: COSMOS_KEY environment variable not set");
    process.exit(1);
  }

  const client = new CosmosClient({ endpoint: COSMOS_ENDPOINT, key: COSMOS_KEY });
  const database = client.database(DATABASE_NAME);

  // Migrate Categories
  console.log("Migrating Categories (normalized)...");
  const categoriesContainer = database.container("Categories");
  for (const cat of categories) {
    try {
      await categoriesContainer.items.upsert(cat);
      console.log(`✓ Category: ${cat.id} - ${cat.name}`);
    } catch (error) {
      console.error(`✗ Category ${cat.id}: ${error.message}`);
    }
  }

  // Migrate Tags
  console.log("\nMigrating Tags (normalized)...");
  const tagsContainer = database.container("Tags");
  for (const tag of tags) {
    try {
      await tagsContainer.items.upsert(tag);
      console.log(`✓ Tag: ${tag.id} - ${tag.name}`);
    } catch (error) {
      console.error(`✗ Tag ${tag.id}: ${error.message}`);
    }
  }

  // Verify counts
  const catCount = await categoriesContainer.items.query("SELECT VALUE COUNT(1) FROM c").fetchAll();
  const tagCount = await tagsContainer.items.query("SELECT VALUE COUNT(1) FROM c").fetchAll();
  
  console.log("\n--- Migration Complete ---");
  console.log(`Categories: ${catCount.resources[0]}`);
  console.log(`Tags: ${tagCount.resources[0]}`);
}

migrate().catch(console.error);
