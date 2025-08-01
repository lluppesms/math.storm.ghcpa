// --------------------------------------------------------------------------------
// Main Bicep file that creates all of the Azure Resources for one environment
// --------------------------------------------------------------------------------
// To deploy this Bicep manually:
// 	 az login
//   az account set --subscription <subscriptionId>
//   az deployment group create -n "manual-$(Get-Date -Format 'yyyyMMdd-HHmmss')" --resource-group rg_Math.Storm_test --template-file 'main.bicep' --parameters appName=xxx-Math.Storm-test environmentCode=demo keyVaultOwnerUserId=xxxxxxxx-xxxx-xxxx
// --------------------------------------------------------------------------------
param appName string = ''
param environmentCode string = 'azd'
param location string = resourceGroup().location
param webSiteSku string = 'B1'
param servicePlanName string = ''
param webAppKind string = 'linux' // 'linux' or 'windows'

param storageSku string = 'Standard_LRS'

// --------------------------------------------------------------------------------------------------------------
// Run Settings Parameters
// --------------------------------------------------------------------------------------------------------------
@description('Should we run a script to dedupe the KeyVault secrets? (this fails on private networks right now)')
param deduplicateKeyVaultSecrets bool = true
@description('Add Role Assignments for the user assigned identity?')
param addRoleAssignments bool = true
@description('Should resources be created with public access?')
param publicAccessEnabled bool = true
@description('Should we deploy Cosmos DB?')
param deployCosmos bool = true

// --------------------------------------------------------------------------------------------------------------
// Personal info Parameters
// --------------------------------------------------------------------------------------------------------------
@description('My IP address for network access')
param myIpAddress string = ''
@description('Id of the user executing the deployment')
param principalId string = ''

// --------------------------------------------------------------------------------------------------------------
// Misc. Parameters
// --------------------------------------------------------------------------------------------------------------
param runDateTime string = utcNow()

// --------------------------------------------------------------------------------
var deploymentSuffix = '-${runDateTime}'
var commonTags = {         
  LastDeployed: runDateTime
  Application: appName
  Environment: environmentCode
}
var resourceGroupName = resourceGroup().name
// var resourceToken = toLower(uniqueString(resourceGroup().id, location))


// --------------------------------------------------------------------------------
module resourceNames 'resourcenames.bicep' = {
  name: 'resourcenames${deploymentSuffix}'
  params: {
    appName: appName
    environmentCode: environmentCode
  }
}
// --------------------------------------------------------------------------------
module logAnalyticsWorkspaceModule 'modules/monitor/loganalytics.bicep' = {
  name: 'logAnalytics${deploymentSuffix}'
  params: {
    newLogAnalyticsName: resourceNames.outputs.logAnalyticsWorkspaceName
    newApplicationInsightsName: resourceNames.outputs.webSiteAppInsightsName
    location: location
    tags: commonTags
  }
}

// --------------------------------------------------------------------------------
var cosmosDatabaseName = 'FuncDemoDatabase'
var gameContainerName = 'Game'
var userContainerName = 'GameUser' 
var leaderboardContainerName = 'LeaderboardEntry'
var cosmosContainerArray = [
  { name: userContainerName, partitionKey: '/userId' }
  { name: gameContainerName, partitionKey: '/gameId' }
  { name: leaderboardContainerName, partitionKey: '/entryId' }
]
module cosmosModule 'modules/database/cosmosdb.bicep' = if (deployCosmos) {
  name: 'cosmos${deploymentSuffix}'
  params: {
    accountName: resourceNames.outputs.cosmosDatabaseName 
    location: location
    tags: commonTags
    containerArray: cosmosContainerArray
    databaseName: cosmosDatabaseName
  }
}

// --------------------------------------------------------------------------------------------------------------
// -- Identity and Access Resources -----------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------------------------
module identity './modules/iam/identity.bicep' = {
  name: 'app-identity${deploymentSuffix}'
  params: {
    identityName: resourceNames.outputs.userAssignedIdentityName
    location: location
  }
}

module appIdentityRoleAssignments './modules/iam/role-assignments.bicep' = if (addRoleAssignments) {
  name: 'identity-roles${deploymentSuffix}'
  params: {
    identityPrincipalId: identity.outputs.managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
    cosmosName: deployCosmos ? cosmosModule.outputs.name : ''
    keyVaultName: keyVaultModule.outputs.name
    storageAccountName: functionStorageModule.outputs.name
  }
}

module adminUserRoleAssignments './modules/iam/role-assignments.bicep' = if (addRoleAssignments && !empty(principalId)) {
  name: 'user-roles${deploymentSuffix}'
  params: {
    identityPrincipalId: principalId
    principalType: 'User'
    cosmosName: deployCosmos ? cosmosModule.outputs.name : ''
    keyVaultName: keyVaultModule.outputs.name
    storageAccountName: functionStorageModule.outputs.name
  }
}

// module functionAppRoleAssignments './modules/iam/role-assignments.bicep' = if (addRoleAssignments) {
//   name: 'function-roles${deploymentSuffix}'
//   params: {
//     identityPrincipalId: functionModule.outputs.functionAppPrincipalId
//     principalType: 'ServicePrincipal'
//     cosmosName: deployCosmos ? cosmosModule.outputs.name : ''
//     keyVaultName: keyVaultModule.outputs.name
//     storageAccountName: functionStorageModule.outputs.name
//   }
// }

// --------------------------------------------------------------------------------
module keyVaultModule './modules/security/keyvault.bicep' = {
  name: 'keyvault${deploymentSuffix}'
  params: {
    location: location
    commonTags: commonTags
    keyVaultName: resourceNames.outputs.keyVaultName
    keyVaultOwnerUserId: principalId
    adminUserObjectIds: [ identity.outputs.managedIdentityPrincipalId ]
    publicNetworkAccess: publicAccessEnabled ? 'Enabled' : 'Disabled'
    keyVaultOwnerIpAddress: myIpAddress
    createUserAssignedIdentity: false
  }
}
module keyVaultSecretList './modules/security/keyvault-list-secret-names.bicep' = if (deduplicateKeyVaultSecrets) {
  name: 'keyVault-Secret-List-Names${deploymentSuffix}'
  params: {
    keyVaultName: keyVaultModule.outputs.name
    location: location
    userManagedIdentityId: identity.outputs.managedIdentityId
  }
}

module keyVaultSecretAppInsights './modules/security/keyvault-secret.bicep' = {
  name: 'keyVaultSecretAppInsights${deploymentSuffix}'
  dependsOn: [ keyVaultModule, logAnalyticsWorkspaceModule, webSiteModule, functionModule ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'appInsightsInstrumentationKey'
    secretValue: logAnalyticsWorkspaceModule.outputs.appInsightsInstrumentationKey
    existingSecretNames: deduplicateKeyVaultSecrets ? keyVaultSecretList!.outputs.secretNameList : ''
  }
}  

module keyVaultSecretCosmos './modules/security/keyvault-cosmos-secret.bicep' = if (deployCosmos) {
  name: 'keyVaultSecretCosmos${deploymentSuffix}'
  dependsOn: [ keyVaultModule, cosmosModule, webSiteModule, functionModule  ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    accountKeySecretName: 'cosmosAccountKey'
    connectionStringSecretName: 'cosmosConnectionString'
    cosmosAccountName: cosmosModule.outputs.name
    existingSecretNames: deduplicateKeyVaultSecrets ? keyVaultSecretList!.outputs.secretNameList : ''
  }
}

// --------------------------------------------------------------------------------
// Service Plan SHARED by webapp and function app
// --------------------------------------------------------------------------------
module appServicePlanModule './modules/webapp/websiteserviceplan.bicep' = {
  name: 'appService${deploymentSuffix}'
  params: {
    location: location
    commonTags: commonTags
    sku: webSiteSku
    environmentCode: environmentCode
    appServicePlanName: servicePlanName == '' ? resourceNames.outputs.webSiteAppServicePlanName : servicePlanName
    existingServicePlanName: servicePlanName
    webAppKind: webAppKind
  }
}

// --------------------------------------------------------------------------------
module webSiteModule './modules/webapp/website.bicep' = {
  name: 'webSite${deploymentSuffix}'
  params: {
    webSiteName: resourceNames.outputs.webSiteName
    location: location
    commonTags: commonTags
    environmentCode: environmentCode
    webAppKind: webAppKind
    workspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    appServicePlanName: appServicePlanModule.outputs.name
    sharedAppInsightsInstrumentationKey: logAnalyticsWorkspaceModule.outputs.appInsightsInstrumentationKey
    managedIdentityId: identity.outputs.managedIdentityId
    managedIdentityPrincipalId: identity.outputs.managedIdentityPrincipalId
  }
}

// In a Linux app service, any nested JSON app key like AppSettings:MyKey needs to be 
// configured in App Service as AppSettings__MyKey for the key name. 
// In other words, any : should be replaced by __ (double underscore).
// NOTE: See https://learn.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal  
module webSiteAppSettingsModule './modules/webapp/websiteappsettings.bicep' = {
  name: 'webSiteAppSettings${deploymentSuffix}'
  params: {
    webAppName: webSiteModule.outputs.name
    appInsightsKey: logAnalyticsWorkspaceModule.outputs.appInsightsInstrumentationKey
    customAppSettings: {
      AppSettings__AppInsights_InstrumentationKey: logAnalyticsWorkspaceModule.outputs.appInsightsInstrumentationKey
      AppSettings__EnvironmentName: environmentCode
      CosmosDb__ConnectionString: deployCosmos ? keyVaultSecretCosmos.outputs.connectionStringSecretUri : ''
      CosmosDb__DatabaseName: resourceNames.outputs.cosmosDatabaseName 
      CosmosDb__ContainerNames__Users: userContainerName
      CosmosDb__ContainerNames__Games: gameContainerName
      CosmosDb__ContainerNames__Leaderboard: leaderboardContainerName
    }
  }
}

// --------------------------------------------------------------------------------
// Function App for API endpoints
// --------------------------------------------------------------------------------
module functionStorageModule './modules/storage/storage-account.bicep' = {
  name: 'functionstorage${deploymentSuffix}'
  params: {
    storageSku: storageSku
    storageAccountName: resourceNames.outputs.functionStorageName
    location: location
    commonTags: commonTags
    allowNetworkAccess:  'Allow'    // 'Deny'
    publicNetworkAccess: 'Enabled'  // 'Disabled'
  }
}

module functionModule './modules/functions/functionapp.bicep' = {
  name: 'function${deploymentSuffix}'
  params: {
    functionAppName: resourceNames.outputs.functionAppName
    sharedAppServicePlanName: appServicePlanModule.outputs.name
    sharedAppInsightsInstrumentationKey: logAnalyticsWorkspaceModule.outputs.appInsightsInstrumentationKey
    sharedAppInsightsConnectionString: logAnalyticsWorkspaceModule.outputs.appInsightsConnectionString
    managedIdentityId: identity.outputs.managedIdentityId
    managedIdentityPrincipalId: identity.outputs.managedIdentityPrincipalId
    keyVaultName: keyVaultModule.outputs.name

    location: location
    commonTags: commonTags

    functionKind: 'functionapp,linux'
    functionStorageAccountName: functionStorageModule.outputs.name
    workspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
  }
}

module functionAppSettingsModule './modules/functions/functionappsettings.bicep' = {
  name: 'functionAppSettings${deploymentSuffix}'
  params: {
    functionAppName: functionModule.outputs.name
    functionStorageAccountName: functionModule.outputs.storageAccountName
    functionInsightsKey: logAnalyticsWorkspaceModule.outputs.appInsightsInstrumentationKey
    keyVaultName: keyVaultModule.outputs.name
    customAppSettings: {
      OpenApi__HideSwaggerUI: 'false'
      OpenApi__HideDocument: 'false'
      OpenApi__DocTitle: 'MathStorm Game APIs'
      OpenApi__DocDescription: 'This repo is an example of a GitHub Copilot Agent Vibe Coded Game'
      appInsightsConnectionString: logAnalyticsWorkspaceModule.outputs.appInsightsConnectionString
      CosmosDb__ConnectionString: deployCosmos ? keyVaultSecretCosmos.outputs.connectionStringSecretUri : ''
      CosmosDb__DatabaseName: resourceNames.outputs.cosmosDatabaseName 
      CosmosDb__ContainerNames__Users: userContainerName
      CosmosDb__ContainerNames__Games: gameContainerName
      CosmosDb__ContainerNames__Leaderboard: leaderboardContainerName
    }
  }
}

// --------------------------------------------------------------------------------
output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP_NAME string = resourceGroupName
output WEB_HOST_NAME string = webSiteModule.outputs.hostName
output FUNCTION_HOST_NAME string = functionModule.outputs.hostname
