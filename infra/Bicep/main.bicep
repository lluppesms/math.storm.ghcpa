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
param servicePlanName string = ''
param servicePlanResourceGroupName string = '' // if using an existing service plan in a different resource group

param webAppKind string = 'linux' // 'linux' or 'windows'
param webSiteSku string = 'B1'

param storageSku string = 'Standard_LRS'
param functionAppSku string = 'B1'
param functionAppSkuFamily string = ''
param functionAppSkuTier string = 'Dynamic'
param environmentSpecificFunctionName string = ''

param OpenAI_Endpoint string
@secure()
param OpenAI_ApiKey string

// --------------------------------------------------------------------------------------------------------------
// Run Settings Parameters
// --------------------------------------------------------------------------------------------------------------
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
// calculated variables disguised as parameters
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
    environmentSpecificFunctionName: environmentSpecificFunctionName
  }
}
// --------------------------------------------------------------------------------
module logAnalyticsWorkspaceModule 'modules/monitor/loganalytics.bicep' = {
  name: 'logAnalytics${deploymentSuffix}'
  params: {
    newLogAnalyticsName: resourceNames.outputs.logAnalyticsWorkspaceName
    newWebApplicationInsightsName: resourceNames.outputs.webSiteAppInsightsName
    newFunctionApplicationInsightsName: resourceNames.outputs.functionAppInsightsName
    location: location
    tags: commonTags
  }
}

// --------------------------------------------------------------------------------
var cosmosDatabaseName = 'MathStormData-${environmentCode}'
var gameContainerName = 'Game'
var userContainerName = 'GameUser' 
var leaderboardContainerName = 'LeaderboardEntry'
var cosmosContainerArray = [
  { name: userContainerName, partitionKey: '/id' }
  { name: gameContainerName, partitionKey: '/id' }
  { name: leaderboardContainerName, partitionKey: '/id' }
]
module cosmosModule 'modules/database/cosmosdb.bicep' = {
  name: 'cosmos${deploymentSuffix}'
  params: {
    accountName: deployCosmos ? resourceNames.outputs.cosmosDatabaseName : ''
    // if this is no, then use the existing cosmos so you don't have to wait 20 minutes every time...
    existingAccountName: deployCosmos ? '' : resourceNames.outputs.cosmosDatabaseName
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
    cosmosName: cosmosModule.outputs.name
    keyVaultName: keyVaultModule.outputs.name
    storageAccountName: functionStorageModule.outputs.name
  }
}

module adminUserRoleAssignments './modules/iam/role-assignments.bicep' = if (addRoleAssignments && !empty(principalId)) {
  name: 'user-roles${deploymentSuffix}'
  params: {
    identityPrincipalId: principalId
    principalType: 'User'
    cosmosName: cosmosModule.outputs.name
    keyVaultName: keyVaultModule.outputs.name
    storageAccountName: functionStorageModule.outputs.name
  }
}

module functionAppRoleAssignments './modules/iam/role-assignments.bicep' = if (addRoleAssignments) {
  name: 'function-roles${deploymentSuffix}'
  params: {
    identityPrincipalId: functionFlexModule.outputs.functionAppPrincipalId
    principalType: 'ServicePrincipal'
    cosmosName: cosmosModule.outputs.name
    keyVaultName: keyVaultModule.outputs.name
    storageAccountName: functionStorageModule.outputs.name
  }
}

// --------------------------------------------------------------------------------
module keyVaultModule './modules/security/keyvault.bicep' = {
  name: 'keyvault${deploymentSuffix}'
  params: {
    location: location
    commonTags: commonTags
    adminUserObjectIds: [ principalId ]
    applicationUserObjectIds: [ identity.outputs.managedIdentityPrincipalId ]
    workspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    publicNetworkAccess: 'Enabled'
    //allowNetworkAccess: 'Allow'
    useRBAC: true
  }
}

module keyVaultSecretAppInsights1 './modules/security/keyvault-secret.bicep' = {
  name: 'keyVaultSecretAppInsights1${deploymentSuffix}'
  dependsOn: [ keyVaultModule, logAnalyticsWorkspaceModule, webSiteModule ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'webAppInsightsInstrumentationKey'
    secretValue: logAnalyticsWorkspaceModule.outputs.webAppInsightsInstrumentationKey
  }
}  
module keyVaultSecretAppInsights2 './modules/security/keyvault-secret.bicep' = {
  name: 'keyVaultSecretAppInsights2${deploymentSuffix}'
  dependsOn: [ keyVaultModule, logAnalyticsWorkspaceModule, functionFlexModule ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'functionAppInsightsInstrumentationKey'
    secretValue: logAnalyticsWorkspaceModule.outputs.functionAppInsightsInstrumentationKey
  }
}  

module keyVaultSecretCosmos './modules/security/keyvault-cosmos-secret.bicep' = {
  name: 'keyVaultSecretCosmos${deploymentSuffix}'
  dependsOn: [ keyVaultModule, cosmosModule, webSiteModule, functionFlexModule  ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    accountKeySecretName: 'cosmosAccountKey'
    connectionStringSecretName: 'cosmosConnectionString'
    cosmosAccountName: cosmosModule.outputs.name
  }
}

module keyVaultSecretFunctionKey './modules/security/keyvault-function-secret.bicep' = {
  name: 'keyVaultSecretFunctionKey${deploymentSuffix}'
  dependsOn: [ keyVaultModule, functionFlexModule ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'functionAppApiKey'
    functionAppName: functionFlexModule.outputs.name
  }
}

// --------------------------------------------------------------------------------
// Service Plan SHARED by webapp and function app
// --------------------------------------------------------------------------------
module appServicePlanModule './modules/webapp/websiteserviceplan.bicep' = {
  name: 'appServicePlan${deploymentSuffix}'
  params: {
    location: location
    commonTags: commonTags
    sku: webSiteSku
    environmentCode: environmentCode
    appServicePlanName: servicePlanName == '' ? resourceNames.outputs.webSiteAppServicePlanName : servicePlanName
    existingServicePlanName: servicePlanName
    existingServicePlanResourceGroupName: servicePlanResourceGroupName
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
    managedIdentityId: identity.outputs.managedIdentityId
    managedIdentityPrincipalId: identity.outputs.managedIdentityPrincipalId
    workspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    appServicePlanName: appServicePlanModule.outputs.name
    appServicePlanResourceGroupName: appServicePlanModule.outputs.resourceGroupName
    sharedAppInsightsInstrumentationKey: logAnalyticsWorkspaceModule.outputs.webAppInsightsInstrumentationKey
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
    appInsightsKey: logAnalyticsWorkspaceModule.outputs.webAppInsightsInstrumentationKey
    customAppSettings: {
      AppSettings__AppInsights_InstrumentationKey: logAnalyticsWorkspaceModule.outputs.webAppInsightsInstrumentationKey
      AppSettings__EnvironmentName: environmentCode
      FunctionService__BaseUrl: 'https://${functionFlexModule.outputs.hostname}'
      FunctionService__APIKey: keyVaultSecretFunctionKey.outputs.secretUri
      FunctionService__MasterKey: 'unknown'
      ConnectionStrings__ApplicationInsights: logAnalyticsWorkspaceModule.outputs.webAppInsightsConnectionString
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
    allowNetworkAccess: true
    allowPublicNetworkAccess: false
    allowSharedKeyAccess: false
  }
}

//--------------------------------------------------------------------------------
module functionFlexModule 'modules/functions/functionflex.bicep' = {
  name: 'functionFlex${deploymentSuffix}'
  dependsOn: [ appIdentityRoleAssignments ]
  params: {
    functionAppName: resourceNames.outputs.functionFlexAppName
    functionAppServicePlanName: resourceNames.outputs.functionFlexAppServicePlanName
    functionInsightsName: resourceNames.outputs.functionFlexInsightsName
    functionStorageAccountName: resourceNames.outputs.functionFlexStorageName
    location: location
    commonTags: commonTags
    workspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    adminPrincipalId: principalId
    deploymentSuffix: deploymentSuffix
  }
}

// module functionModule './modules/functions/functionapp.bicep' = {
//   name: 'function${deploymentSuffix}'
//   dependsOn: [ appIdentityRoleAssignments ]
//   params: {
//     functionAppName: resourceNames.outputs.functionAppName
//     functionAppServicePlanName: resourceNames.outputs.functionAppServicePlanName
//     functionInsightsName: resourceNames.outputs.functionAppInsightsName
//     sharedAppServicePlanName: appServicePlanModule.outputs.name
//     sharedAppInsightsInstrumentationKey: logAnalyticsWorkspaceModule.outputs.functionAppInsightsInstrumentationKey
//     sharedAppInsightsConnectionString: logAnalyticsWorkspaceModule.outputs.functionAppInsightsConnectionString
//     // switch to system assigned principal for secure storage access...
//     // keyVaultName: keyVaultModule.outputs.name
//     managedIdentityId: identity.outputs.managedIdentityId
//     managedIdentityPrincipalId: identity.outputs.managedIdentityPrincipalId

//     location: location
//     commonTags: commonTags

//     functionKind: 'functionapp,linux'
//     functionAppSku: functionAppSku
//     functionAppSkuFamily: functionAppSkuFamily
//     functionAppSkuTier: functionAppSkuTier
//     functionStorageAccountName: functionStorageModule.outputs.name
//     workspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
//   }
// }
// resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
//   name: keyVaultModule.outputs.name
// } 
module functionAppSettingsModule './modules/functions/functionappsettings.bicep' = {
  name: 'functionAppSettings${deploymentSuffix}'
  params: {
    functionAppName: functionFlexModule.outputs.name
    functionStorageAccountName: functionFlexModule.outputs.storageAccountName
    functionInsightsKey: logAnalyticsWorkspaceModule.outputs.functionAppInsightsInstrumentationKey
    // keyVaultName: keyVaultModule.outputs.name

    cosmosAccountName: cosmosModule.outputs.name

    OpenAI_Gpt4o_DeploymentName: 'gpt-4o-mini'
    OpenAI_Gpt4o_Endpoint: OpenAI_Endpoint
    OpenAI_Gpt4o_ApiKey: OpenAI_ApiKey
    OpenAI_Gpt35_DeploymentName: 'gpt-35-turbo'
    OpenAI_Gpt35_Endpoint: OpenAI_Endpoint
    OpenAI_Gpt35_ApiKey: OpenAI_ApiKey

    customAppSettings: {
      OpenApi__HideSwaggerUI: 'false'
      OpenApi__HideDocument: 'false'
      OpenApi__DocTitle: 'MathStorm Game APIs'
      OpenApi__DocDescription: 'This repo is an example of a GitHub Copilot Agent Vibe Coded Game'
      appInsightsConnectionString: logAnalyticsWorkspaceModule.outputs.functionAppInsightsConnectionString
      CosmosDb__DatabaseName: cosmosDatabaseName 
      CosmosDb__ContainerNames__Users: userContainerName
      CosmosDb__ContainerNames__Games: gameContainerName
      CosmosDb__ContainerNames__Leaderboard: leaderboardContainerName
      OpenAI__DefaultModel: 'gpt_4o_mini' // must use underscores, not hyphens...
      OpenAI__Temperature: '0.8'     
    }
  }
}

// --------------------------------------------------------------------------------
output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP_NAME string = resourceGroupName
output WEB_HOST_NAME string = webSiteModule.outputs.hostName
//output FUNCTION_HOST_NAME string = functionModule.outputs.hostname
output FLEX_FUNCTION_HOST_NAME string = functionFlexModule.outputs.hostname
