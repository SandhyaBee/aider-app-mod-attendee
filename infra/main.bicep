// StyleVerse Azure Infrastructure - Phase 4: Global Distribution
// Provisions: Multi-region App Services behind Azure Front Door

@description('The name prefix used for all resources')
param projectName string = 'styleverse'

@description('The App Service Plan SKU')
@allowed(['F1', 'B1', 'B2', 'B3', 'S1', 'S2', 'S3', 'P1v2', 'P2v2', 'P3v2'])
param appServicePlanSku string = 'B1'

@description('Cosmos DB account endpoint')
param cosmosDbEndpoint string

@secure()
@description('Cosmos DB account key')
param cosmosDbKey string

var uniqueSuffix = uniqueString(resourceGroup().id)

var regions = [
  {
    name: 'germanywestcentral'
    displayName: 'Germany West Central'
  }
  {
    name: 'eastus2'
    displayName: 'East US 2'
  }
  {
    name: 'eastasia'
    displayName: 'East Asia'
  }
  {
    name: 'francecentral'
    displayName: 'France Central'
  }
]

// ─── App Service Plans (one per region) ─────────────────────────

resource appServicePlans 'Microsoft.Web/serverfarms@2023-01-01' = [for region in regions: {
  name: '${projectName}-plan-${region.name}-${uniqueSuffix}'
  location: region.name
  sku: {
    name: appServicePlanSku
    capacity: 1
  }
}]

// ─── Web Apps (one per region) ──────────────────────────────────

resource webApps 'Microsoft.Web/sites@2023-01-01' = [for (region, i) in regions: {
  name: '${projectName}-app-${region.name}-${uniqueSuffix}'
  location: region.name
  properties: {
    serverFarmId: appServicePlans[i].id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      alwaysOn: appServicePlanSku != 'F1'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'REGION_NAME'
          value: region.displayName
        }
        {
          name: 'COSMOS_PREFERRED_REGION'
          value: region.displayName
        }
        {
          name: 'CosmosDb__Endpoint'
          value: cosmosDbEndpoint
        }
        {
          name: 'CosmosDb__Key'
          value: cosmosDbKey
        }
        {
          name: 'CosmosDb__DatabaseName'
          value: 'StyleVerseDb'
        }
      ]
    }
    httpsOnly: true
  }
}]

// ─── Azure Front Door ───────────────────────────────────────────

resource frontDoorProfile 'Microsoft.Cdn/profiles@2024-02-01' = {
  name: '${projectName}-fd-${uniqueSuffix}'
  location: 'global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
}

resource frontDoorEndpoint 'Microsoft.Cdn/profiles/afdEndpoints@2024-02-01' = {
  parent: frontDoorProfile
  name: '${projectName}-endpoint'
  location: 'global'
  properties: {
    enabledState: 'Enabled'
  }
}

resource originGroup 'Microsoft.Cdn/profiles/originGroups@2024-02-01' = {
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

resource origins 'Microsoft.Cdn/profiles/originGroups/origins@2024-02-01' = [for (region, i) in regions: {
  parent: originGroup
  name: 'origin-${region.name}'
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

resource route 'Microsoft.Cdn/profiles/afdEndpoints/routes@2024-02-01' = {
  parent: frontDoorEndpoint
  name: 'default-route'
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
  }
  dependsOn: [
    origins
  ]
}

// ─── Outputs ────────────────────────────────────────────────────

output frontDoorEndpointHostName string = frontDoorEndpoint.properties.hostName
output webAppNames array = [for (region, i) in regions: webApps[i].name]
output webAppUrls array = [for (region, i) in regions: 'https://${webApps[i].properties.defaultHostName}']
