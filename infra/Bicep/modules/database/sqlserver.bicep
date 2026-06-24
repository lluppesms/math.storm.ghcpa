// --------------------------------------------------------------------------------
// This BICEP file will create an Azure SQL Server and Database
// for the MathStorm application using the mathstorm schema
// --------------------------------------------------------------------------------
param sqlServerName string = 'sql-${uniqueString(resourceGroup().id)}'
param sqlDBName string = 'MathStormDB'
param existingSqlServerName string = ''
param existingSqlServerResourceGroupName string = ''

param adAdminUserId string = ''
param adAdminUserSid string = ''
param adAdminTenantId string = ''
param userAssignedIdentityResourceId string = ''
param location string = resourceGroup().location
param commonTags object = {}

// Serverless GeneralPurpose config
@allowed(['Basic', 'Standard', 'Premium', 'BusinessCritical', 'GeneralPurpose'])
param sqlSkuTier string = 'GeneralPurpose'
param sqlSkuFamily string = 'Gen5'
param sqlSkuName string = 'GP_S_Gen5'
param minCores int = 1
param autoPause int = 60

@description('The workspace to store audit logs.')
param workspaceId string = ''

param sqlAdminUser string = ''
@secure()
param sqlAdminPassword string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~sqlserver.bicep' }
var tags = union(commonTags, templateTag)

var useSqlAuth = !empty(sqlAdminPassword)
var adAdminOnly = !useSqlAuth
var adminDefinition = adAdminUserId == '' ? {} : {
  administratorType: 'ActiveDirectory'
  principalType: 'Group'
  login: adAdminUserId
  sid: adAdminUserSid
  tenantId: adAdminTenantId
  azureADOnlyAuthentication: adAdminOnly
}

var deployNewServer = empty(existingSqlServerName)

// --------------------------------------------------------------------------------
resource existingSqlServerResource 'Microsoft.Sql/servers@2024-11-01-preview' existing = if (!deployNewServer) {
  name: existingSqlServerName
  scope: resourceGroup(existingSqlServerResourceGroupName)
}
resource existingSqlDBResource 'Microsoft.Sql/servers/databases@2024-11-01-preview' existing = if (!deployNewServer) {
  parent: existingSqlServerResource
  name: sqlDBName
}

resource sqlServerResource 'Microsoft.Sql/servers@2024-11-01-preview' = if (deployNewServer) {
  name: sqlServerName
  location: location
  tags: tags
  properties: {
    administrators: adminDefinition
    primaryUserAssignedIdentityId: !empty(userAssignedIdentityResourceId) ? userAssignedIdentityResourceId : null
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
    administratorLogin: !empty(sqlAdminUser) ? sqlAdminUser : null
    administratorLoginPassword: !empty(sqlAdminPassword) ? sqlAdminPassword : null
  }
  identity: !empty(userAssignedIdentityResourceId) ? {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityResourceId}': {}
    }
  } : {
    type: 'SystemAssigned'
  }
}

resource sqlDBResource 'Microsoft.Sql/servers/databases@2024-11-01-preview' = if (deployNewServer) {
  parent: sqlServerResource
  name: sqlDBName
  location: location
  tags: tags
  sku: {
    name: sqlSkuName
    tier: sqlSkuTier
    family: sqlSkuFamily
    capacity: 2
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 4294967296 // 4 GB
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    autoPauseDelay: autoPause
    requestedBackupStorageRedundancy: 'Geo'
    minCapacity: minCores
    isLedgerOn: false
  }
}

// Allow all Azure services to access this server
resource sqlAllowAllAzureIps 'Microsoft.Sql/servers/firewallRules@2024-11-01-preview' = if (deployNewServer) {
  name: 'AllowAllWindowsAzureIps'
  parent: sqlServerResource
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (deployNewServer && !empty(workspaceId)) {
  scope: sqlDBResource
  name: 'SQLSecurityAuditEvents'
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'SQLSecurityAuditEvents'
        enabled: true
        retentionPolicy: { days: 0, enabled: false }
      }
      {
        category: 'DevOpsOperationsAudit'
        enabled: true
        retentionPolicy: { days: 0, enabled: false }
      }
    ]
  }
}

resource sqlDBAuditingSettings 'Microsoft.Sql/servers/auditingSettings@2024-11-01-preview' = if (deployNewServer) {
  parent: sqlServerResource
  name: 'default'
  properties: {
    retentionDays: 7
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    isAzureMonitorTargetEnabled: true
    state: 'Enabled'
  }
}

// --------------------------------------------------------------------------------
var outputServerName = deployNewServer ? sqlServerResource.name : existingSqlServerResource.name
var outputDatabaseName = deployNewServer ? sqlDBResource.name : existingSqlDBResource.name

output serverName string = outputServerName
output serverId string = deployNewServer ? sqlServerResource.id : existingSqlServerResource.id
output databaseName string = outputDatabaseName
output databaseId string = deployNewServer ? sqlDBResource.id : existingSqlDBResource.id
output identityConnectionString string = 'Server=tcp:${outputServerName}.database.windows.net,1433;Initial Catalog=${outputDatabaseName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;Authentication="Active Directory Default";'
