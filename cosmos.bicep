@description('The Cosmos DB account name')
param cosmosAccountName string

@description('Primary location for the Cosmos DB account')
param primaryLocation string = 'germanywestcentral'

var databaseName = 'StyleVerseDb'

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: cosmosAccountName
  location: primaryLocation
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: true
    locations: [
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
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
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
    }
    options: {
      throughput: 400
    }
  }
}

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
    }
    options: {
      throughput: 400
    }
  }
}

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
      throughput: 400
    }
  }
}

output cosmosAccountEndpoint string = cosmosAccount.properties.documentEndpoint
output cosmosAccountName string = cosmosAccount.name
