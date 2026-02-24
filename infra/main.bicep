// StyleVerse Azure Infrastructure - Phase 1
// Provisions: Azure SQL Database, App Service Plan, and Web App in UK South

@description('The name prefix used for all resources')
param projectName string = 'styleverse'

@description('The location for all resources')
param location string = 'germanywestcentral'

@description('The SQL Server administrator login')
param sqlAdminLogin string

@secure()
@description('The SQL Server administrator password')
param sqlAdminPassword string

@description('The App Service Plan SKU')
@allowed(['F1', 'B1', 'B2', 'B3', 'S1', 'S2', 'S3', 'P1v2', 'P2v2', 'P3v2'])
param appServicePlanSku string = 'S1'

// Unique suffix for globally unique resource names
var uniqueSuffix = uniqueString(resourceGroup().id)
var sqlServerName = '${projectName}-sql-${uniqueSuffix}'
var sqlDatabaseName = '${projectName}db'
var appServicePlanName = '${projectName}-plan-${uniqueSuffix}'
var webAppName = '${projectName}-app-${uniqueSuffix}'

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    publicNetworkAccess: 'Enabled'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
  }
}

// Firewall rule to allow Azure services (required for App Service connectivity)
resource sqlFirewallAzureServices 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
    capacity: 1
  }
  properties: {
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      alwaysOn: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
    }
    httpsOnly: true
  }
}

// Connection string configuration for the Web App
resource webAppConnectionStrings 'Microsoft.Web/sites/config@2023-01-01' = {
  parent: webApp
  name: 'connectionstrings'
  properties: {
    DefaultConnection: {
      value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
      type: 'SQLAzure'
    }
  }
}

// Outputs for deployment scripts
output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabaseName
output sqlServerName string = sqlServer.name
