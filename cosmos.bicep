// StyleVerse Cosmos DB - Phase 4: Multi-Region Global Distribution

@description('Name of the Cosmos DB account')
param cosmosAccountName string

@description('Primary region for the Cosmos DB account')
param primaryLocation string = 'germanywestcentral'

var databaseName = 'StyleVerseDb'

// Multi-region locations with failover priorities
var locations = [
  {
    locationName: primaryLocation
    failoverPriority: 0
    isZoneRedundant: false
  }
  {
    locationName: 'eastus2'
    failoverPriority: 1
    isZoneRedundant: false
  }
  {
    locationName: 'eastasia'
    failoverPriority: 2
    isZoneRedundant: false
  }
  {
    locationName: 'francecentral'
    failoverPriority: 3
    isZoneRedundant: false
  }
]

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: cosmosAccountName
  location: primaryLocation
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: locations
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    enableAutomaticFailover: true
    enableMultipleWriteLocations: true
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

// Products container - partitioned by categoryId
resource productsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: database
  name: 'Products'
  properties: {
    resource: {
      id: 'Products'
      partitionKey: {
        paths: ['/categoryId']
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          { path: '/*' }
        ]
        excludedPaths: [
          { path: '/"_etag"/?' }
        ]
      }
    }
    options: {
      autoscaleSettings: {
        maxThroughput: 4000
      }
    }
  }
}

// CartItems container - partitioned by sessionId
resource cartItemsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: database
  name: 'CartItems'
  properties: {
    resource: {
      id: 'CartItems'
      partitionKey: {
        paths: ['/sessionId']
        kind: 'Hash'
      }
      defaultTtl: 86400 // Cart items expire after 24 hours
    }
    options: {
      autoscaleSettings: {
        maxThroughput: 4000
      }
    }
  }
}

// Orders container - partitioned by shippingRegion
resource ordersContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: database
  name: 'Orders'
  properties: {
    resource: {
      id: 'Orders'
      partitionKey: {
        paths: ['/shippingRegion']
        kind: 'Hash'
      }
    }
    options: {
      autoscaleSettings: {
        maxThroughput: 4000
      }
    }
  }
}

// Outputs for wiring into App Service configuration
output cosmosEndpoint string = cosmosAccount.properties.documentEndpoint
output cosmosDatabaseName string = databaseName
output cosmosAccountName string = cosmosAccount.name
