// StyleVerse Azure Infrastructure - Phase 4: Multi-Region + Front Door
// Provisions: Multi-region App Services, Azure Front Door, Cosmos DB integration

@description('The name prefix used for all resources')
param projectName string = 'styleverse'

@description('Cosmos DB account name')
param cosmosAccountName string

@secure()
@description('Cosmos DB account key')
param cosmosKey string

@description('Cosmos DB database name')
param cosmosDatabaseName string = 'StyleVerseDb'

@description('The App Service Plan SKU')
@allowed(['F1', 'B1', 'B2', 'B3', 'S1', 'S2', 'S3', 'P1v2', 'P2v2', 'P3v2'])
param appServicePlanSku string = 'B1'

// Regions for multi-region deployment
var regions = [
  {
    name: 'germanywestcentral'
    displayName: 'Germany West Central'
    suffix: 'gwc'
  }
  {
    name: 'eastus2'
    displayName: 'East US 2'
    suffix: 'eus2'
  }
  {
    name: 'eastasia'
    displayName: 'East Asia'
    suffix: 'ea'
  }
]

// Unique suffix for globally unique resource names
var uniqueSuffix = uniqueString(resourceGroup().id)

// Reference existing Cosmos DB account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: cosmosAccountName
}

// App Service Plan per region
resource appServicePlans 'Microsoft.Web/serverfarms@2023-01-01' = [for region in regions: {
  name: '${projectName}-plan-${region.suffix}-${uniqueSuffix}'
  location: region.name
  sku: {
    name: appServicePlanSku
    capacity: 1
  }
  properties: {
    reserved: true
  }
  kind: 'linux'
}]

// Web App per region
resource webApps 'Microsoft.Web/sites@2023-01-01' = [for (region, i) in regions: {
  name: '${projectName}-app-${region.suffix}-${uniqueSuffix}'
  location: region.name
  properties: {
    serverFarmId: appServicePlans[i].id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'CosmosDb__Endpoint'
          value: cosmosAccount.properties.documentEndpoint
        }
        {
          name: 'CosmosDb__Key'
          value: cosmosKey
        }
        {
          name: 'CosmosDb__DatabaseName'
          value: cosmosDatabaseName
        }
        {
          name: 'COSMOS_PREFERRED_REGION'
          value: region.displayName
        }
        {
          name: 'REGION_NAME'
          value: region.name
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
    httpsOnly: true
  }
}]

// Azure Front Door Profile
resource frontDoorProfile 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: '${projectName}-fd-${uniqueSuffix}'
  location: 'global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
}

// Front Door Endpoint
resource frontDoorEndpoint 'Microsoft.Cdn/profiles/afdEndpoints@2023-05-01' = {
  parent: frontDoorProfile
  name: '${projectName}-endpoint'
  location: 'global'
  properties: {
    enabledState: 'Enabled'
  }
}

// Origin Group with health probes
resource originGroup 'Microsoft.Cdn/profiles/originGroups@2023-05-01' = {
  parent: frontDoorProfile
  name: '${projectName}-origin-group'
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }
    healthProbeSettings: {
      probePath: '/api/health'
      probeRequestType: 'GET'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 30
    }
    sessionAffinityState: 'Disabled'
  }
}

// Origins - one per regional Web App
resource origins 'Microsoft.Cdn/profiles/originGroups/origins@2023-05-01' = [for (region, i) in regions: {
  parent: originGroup
  name: '${projectName}-origin-${region.suffix}'
  properties: {
    hostName: webApps[i].properties.defaultHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: webApps[i].properties.defaultHostName
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
  }
}]

// Route - map all traffic to origin group
resource route 'Microsoft.Cdn/profiles/afdEndpoints/routes@2023-05-01' = {
  parent: frontDoorEndpoint
  name: '${projectName}-route'
  properties: {
    originGroup: {
      id: originGroup.id
    }
    supportedProtocols: [
      'Http'
      'Https'
    ]
    patternsToMatch: [
      '/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
    cacheConfiguration: {
      queryStringCachingBehavior: 'UseQueryString'
      compressionSettings: {
        isCompressionEnabled: true
        contentTypesToCompress: [
          'application/json'
          'text/html'
          'text/css'
          'application/javascript'
          'text/javascript'
          'application/x-javascript'
          'text/plain'
          'image/svg+xml'
        ]
      }
    }
  }
  dependsOn: [
    origins
  ]
}

// Outputs
output frontDoorEndpointHostName string = frontDoorEndpoint.properties.hostName
output frontDoorEndpointUrl string = 'https://${frontDoorEndpoint.properties.hostName}'
output webAppUrls array = [for (region, i) in regions: {
  region: region.name
  url: 'https://${webApps[i].properties.defaultHostName}'
  name: webApps[i].name
}]
