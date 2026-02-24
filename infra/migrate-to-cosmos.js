// Migration Script: SQL to Cosmos DB
// Uses upsert to avoid conflicts if run multiple times or by multiple users

const { CosmosClient } = require("@azure/cosmos");

// Configuration - using existing resources
const COSMOS_ENDPOINT = "https://styleverse-cosmos-06.documents.azure.com:443/";
const COSMOS_KEY = process.env.COSMOS_KEY;
const DATABASE_NAME = "StyleVerseDb";
const CONTAINER_NAME = "Products";

// Categories lookup (from database.sql)
const categories = {
  1: "Streetwear",
  2: "Formal", 
  3: "Accessories",
  4: "Techwear",
  5: "Footwear"
};

// Tags lookup (from database.sql)
const tags = {
  1: "Cotton",
  2: "Unisex",
  3: "Limited Edition",
  4: "Summer",
  5: "Waterproof",
  6: "Recycled",
  7: "Oversized",
  8: "Neon",
  9: "Breathable",
  10: "Windproof"
};

// Product-Tags mapping (from database.sql)
const productTags = {
  1: [1, 2, 7],    // Galaxy Hoodie: Cotton, Unisex, Oversized
  2: [8, 5],       // Neon Joggers: Neon, Waterproof
  10: [3],         // Midnight Silk Dress: Limited Edition
  11: [3],         // Velvet Tuxedo Jacket: Limited Edition
  28: [5, 10, 9],  // Apex Hardshell: Waterproof, Windproof, Breathable
  37: [8, 9],      // Nebula Runners: Neon, Breathable
  40: [8, 3]       // Circuit High-Tops: Neon, Limited Edition
};

// Products data (from database.sql) - denormalized with embedded category and tags
const products = [
  // Streetwear (Category 1)
  { id: "1", name: "Galaxy Hoodie", description: "100% Cotton, heavy-weight oversized fit.", price: 59.99, categoryId: 1, inventoryCount: 150 },
  { id: "2", name: "Neon Joggers", description: "Reflective street styling for night city life.", price: 75.00, categoryId: 1, inventoryCount: 80 },
  { id: "3", name: "Retro Logo Tee", description: "Vintage wash with 80s inspired graphics.", price: 35.00, categoryId: 1, inventoryCount: 200 },
  { id: "4", name: "Acid Wash Denim", description: "Distressed classic fit denim jacket.", price: 110.00, categoryId: 1, inventoryCount: 40 },
  { id: "5", name: "Skater Cargoes", description: "Wide-leg utility pants with 8 pockets.", price: 85.50, categoryId: 1, inventoryCount: 120 },
  { id: "6", name: "Graffiti Windbreaker", description: "Lightweight shell with artist collab print.", price: 95.00, categoryId: 1, inventoryCount: 60 },
  { id: "7", name: "Boxy Flannel", description: "Brushed wool blend for layering.", price: 65.00, categoryId: 1, inventoryCount: 90 },
  { id: "8", name: "Street Beanie", description: "Soft ribbed acrylic in various colors.", price: 25.00, categoryId: 1, inventoryCount: 300 },
  { id: "9", name: "Vanguard Vest", description: "Utility tactical vest for urban layering.", price: 120.00, categoryId: 1, inventoryCount: 30 },
  
  // Formal (Category 2)
  { id: "10", name: "Midnight Silk Dress", description: "Elegance redefined in pure silk.", price: 250.00, categoryId: 2, inventoryCount: 25 },
  { id: "11", name: "Velvet Tuxedo Jacket", description: "Deep emerald green velvet with satin lapels.", price: 320.00, categoryId: 2, inventoryCount: 15 },
  { id: "12", name: "Oxford Button-Down", description: "Egyptian cotton slim-fit formal shirt.", price: 85.00, categoryId: 2, inventoryCount: 100 },
  { id: "13", name: "Tapered Wool Slacks", description: "Italian wool blend, tailored for comfort.", price: 140.00, categoryId: 2, inventoryCount: 50 },
  { id: "14", name: "Satin Evening Gown", description: "Floor length with a dramatic slit.", price: 400.00, categoryId: 2, inventoryCount: 10 },
  { id: "15", name: "Charcoal Three-Piece", description: "The ultimate professional suit set.", price: 550.00, categoryId: 2, inventoryCount: 20 },
  { id: "16", name: "Silk Pocket Square", description: "Hand-rolled edges with geometric patterns.", price: 30.00, categoryId: 2, inventoryCount: 200 },
  { id: "17", name: "Lace Cocktail Dress", description: "Intricate floral lace over nude lining.", price: 180.00, categoryId: 2, inventoryCount: 35 },
  { id: "18", name: "Cufflink Set - Silver", description: "Engraved minimalist architectural design.", price: 55.00, categoryId: 2, inventoryCount: 150 },
  
  // Accessories (Category 3)
  { id: "19", name: "Urban Snapback", description: "Structured 6-panel with 3D embroidery.", price: 28.00, categoryId: 3, inventoryCount: 500 },
  { id: "20", name: "Cyber Visor", description: "LED-integrated eyewear for the bold.", price: 150.00, categoryId: 3, inventoryCount: 40 },
  { id: "21", name: "Leather Utility Belt", description: "Modular pouch system for urban travel.", price: 95.00, categoryId: 3, inventoryCount: 75 },
  { id: "22", name: "Aviator Shades", description: "Classic gold frames with polarized tint.", price: 120.00, categoryId: 3, inventoryCount: 60 },
  { id: "23", name: "Smart Fabric Scarf", description: "Changes color based on temperature.", price: 85.00, categoryId: 3, inventoryCount: 110 },
  { id: "24", name: "Canvas Tote - StyleVerse", description: "Reinforced eco-friendly daily shopper.", price: 20.00, categoryId: 3, inventoryCount: 1000 },
  { id: "25", name: "Enamel Pin Set", description: "Custom icons from the StyleVerse lore.", price: 15.00, categoryId: 3, inventoryCount: 400 },
  { id: "26", name: "Titanium Chain", description: "Industrial grade chunky neckwear.", price: 130.00, categoryId: 3, inventoryCount: 25 },
  { id: "27", name: "Tech Glove Liners", description: "Touchscreen compatible thin liners.", price: 35.00, categoryId: 3, inventoryCount: 200 },
  
  // Techwear (Category 4)
  { id: "28", name: "Apex Hardshell", description: "Triple-layer Gore-Tex compatible shell.", price: 450.00, categoryId: 4, inventoryCount: 15 },
  { id: "29", name: "Modular Poncho", description: "Convertible silhouette for extreme weather.", price: 210.00, categoryId: 4, inventoryCount: 30 },
  { id: "30", name: "Paratrooper Pants", description: "Reinforced knees and quick-release straps.", price: 195.00, categoryId: 4, inventoryCount: 45 },
  { id: "31", name: "Stealth Mask", description: "Activated carbon filter with mesh exterior.", price: 45.00, categoryId: 4, inventoryCount: 250 },
  { id: "32", name: "Thermal Base Layer", description: "Moisture-wicking compression tech.", price: 70.00, categoryId: 4, inventoryCount: 150 },
  { id: "33", name: "Signal Blocker Pouch", description: "RFID shielding for mobile devices.", price: 40.00, categoryId: 4, inventoryCount: 300 },
  { id: "34", name: "Exo-Sling Bag", description: "Ergonomic cross-body with tablet sleeve.", price: 140.00, categoryId: 4, inventoryCount: 80 },
  { id: "35", name: "Liquid Metal Jersey", description: "High-shine synthetic with UV protection.", price: 90.00, categoryId: 4, inventoryCount: 100 },
  { id: "36", name: "Ventilated Vest", description: "Laser-cut perforations for max airflow.", price: 110.00, categoryId: 4, inventoryCount: 55 },
  
  // Footwear (Category 5)
  { id: "37", name: "Nebula Runners", description: "Glow-in-the-dark soles, mesh upper.", price: 160.00, categoryId: 5, inventoryCount: 120 },
  { id: "38", name: "Titan Combat Boots", description: "Composite toe with waterproof zip.", price: 220.00, categoryId: 5, inventoryCount: 40 },
  { id: "39", name: "Lunar Sandals", description: "Memory foam footbed with neoprene straps.", price: 65.00, categoryId: 5, inventoryCount: 200 },
  { id: "40", name: "Circuit High-Tops", description: "Electronic integrated color-changing strips.", price: 300.00, categoryId: 5, inventoryCount: 20 },
  { id: "41", name: "Urban Suede Loafers", description: "Premium leather interior, daily comfort.", price: 140.00, categoryId: 5, inventoryCount: 65 },
  { id: "42", name: "Track Spike Pro", description: "Carbon fiber plate for explosive speed.", price: 250.00, categoryId: 5, inventoryCount: 30 },
  { id: "43", name: "Cloud Sliders", description: "Single-mold EVA foam for post-gym.", price: 45.00, categoryId: 5, inventoryCount: 400 },
  { id: "44", name: "Vintage Court Shoes", description: "Leather heritage design, gum sole.", price: 95.00, categoryId: 5, inventoryCount: 150 },
  { id: "45", name: "Cyber Sock-Shoe", description: "Knitted upper with rugged rubber outsole.", price: 135.00, categoryId: 5, inventoryCount: 90 }
];

// Transform product to denormalized document
function transformProduct(product) {
  const categoryName = categories[product.categoryId];
  const tagIds = productTags[parseInt(product.id)] || [];
  const productTagsList = tagIds.map(tagId => ({
    id: tagId,
    name: tags[tagId]
  }));
  
  return {
    id: product.id,
    name: product.name,
    description: product.description,
    price: product.price,
    categoryId: product.categoryId.toString(), // Partition key
    category: {
      id: product.categoryId,
      name: categoryName
    },
    tags: productTagsList,
    inventoryCount: product.inventoryCount,
    createdDate: new Date().toISOString()
  };
}

async function migrate() {
  if (!COSMOS_KEY) {
    console.error("ERROR: COSMOS_KEY environment variable not set");
    console.log("Run: export COSMOS_KEY='<your-cosmos-key>'");
    process.exit(1);
  }
  
  const client = new CosmosClient({ endpoint: COSMOS_ENDPOINT, key: COSMOS_KEY });
  const database = client.database(DATABASE_NAME);
  const container = database.container(CONTAINER_NAME);
  
  console.log(`Migrating ${products.length} products to Cosmos DB...`);
  console.log(`Endpoint: ${COSMOS_ENDPOINT}`);
  console.log(`Database: ${DATABASE_NAME}, Container: ${CONTAINER_NAME}`);
  console.log("Using UPSERT to avoid conflicts with parallel migrations.\n");
  
  let success = 0;
  let skipped = 0;
  
  for (const product of products) {
    const document = transformProduct(product);
    try {
      // Upsert - creates if not exists, updates if exists (no conflicts)
      await container.items.upsert(document);
      console.log(`✓ Upserted: ${document.id} - ${document.name}`);
      success++;
    } catch (error) {
      console.error(`✗ Failed: ${document.id} - ${document.name}: ${error.message}`);
      skipped++;
    }
  }
  
  console.log(`\n--- Migration Complete ---`);
  console.log(`Successful: ${success}`);
  console.log(`Failed: ${skipped}`);
  
  // Verify count
  const { resources } = await container.items.query("SELECT VALUE COUNT(1) FROM c").fetchAll();
  console.log(`Total documents in container: ${resources[0]}`);
}

migrate().catch(console.error);
