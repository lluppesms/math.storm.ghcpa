// --------------------------------------------------------------------------------
// Main Bicep file that creates all of the Azure Resources for one environment
// After refactoring: Web App uses SQL Server for storage instead of Cosmos DB
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

param OpenAI_Endpoint string
@secure()
param OpenAI_ApiKey string

// --------------------------------------------------------------------------------------------------------------
// SQL Server Parameters
// --------------------------------------------------------------------------------------------------------------
param sqlAdminUser string = ''
@secure()
param sqlAdminPassword string = ''
@description('Set to false to reuse an existing SQL Server (avoids waiting for a new server to be provisioned)')
param deploySqlServer bool = true

// --------------------------------------------------------------------------------------------------------------
// Run Settings Parameters
// --------------------------------------------------------------------------------------------------------------
@description('Add Role Assignments for the user assigned identity?')
param addRoleAssignments bool = true

// --------------------------------------------------------------------------------------------------------------
// Personal info Parameters
// --------------------------------------------------------------------------------------------------------------
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
    newWebApplicationInsightsName: resourceNames.outputs.webSiteAppInsightsName
    location: location
    tags: commonTags
  }
}

// --------------------------------------------------------------------------------
// SQL Server Database
// --------------------------------------------------------------------------------
module sqlModule 'modules/database/sqlserver.bicep' = {
  name: 'sqlServer${deploymentSuffix}'
  params: {
    sqlServerName: deploySqlServer ? resourceNames.outputs.sqlServerName : ''
    existingSqlServerName: deploySqlServer ? '' : resourceNames.outputs.sqlServerName
    sqlDBName: resourceNames.outputs.sqlDatabaseName
    location: location
    commonTags: commonTags
    userAssignedIdentityResourceId: identity.outputs.managedIdentityId
    workspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    sqlAdminUser: sqlAdminUser
    sqlAdminPassword: sqlAdminPassword
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
    keyVaultName: keyVaultModule.outputs.name
  }
}

module adminUserRoleAssignments './modules/iam/role-assignments.bicep' = if (addRoleAssignments && !empty(principalId)) {
  name: 'user-roles${deploymentSuffix}'
  params: {
    identityPrincipalId: principalId
    principalType: 'User'
    keyVaultName: keyVaultModule.outputs.name
  }
}

// --------------------------------------------------------------------------------
module keyVaultModule './modules/security/keyvault.bicep' = {
  name: 'keyvault${deploymentSuffix}'
  params: {
    keyVaultName: resourceNames.outputs.keyVaultName
    location: location
    commonTags: commonTags
    adminUserObjectIds: [ principalId ]
    applicationUserObjectIds: [ identity.outputs.managedIdentityPrincipalId ]
    workspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    publicNetworkAccess: 'Enabled'
    useRBAC: true
  }
}

module keyVaultSecretAppInsights './modules/security/keyvault-secret.bicep' = {
  name: 'keyVaultSecretAppInsights${deploymentSuffix}'
  dependsOn: [ keyVaultModule, logAnalyticsWorkspaceModule, webSiteModule ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'webAppInsightsInstrumentationKey'
    secretValue: logAnalyticsWorkspaceModule.outputs.webAppInsightsInstrumentationKey
  }
}

module keyVaultSecretSql './modules/security/keyvault-sql-secret.bicep' = {
  name: 'keyVaultSecretSql${deploymentSuffix}'
  dependsOn: [ keyVaultModule, sqlModule, webSiteModule ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'sqlConnectionString'
    sqlServerName: sqlModule.outputs.serverName
    sqlDBName: sqlModule.outputs.databaseName
    sqlAdminUser: sqlAdminUser
    sqlAdminPassword: sqlAdminPassword
  }
}

module keyVaultSecretOpenAI './modules/security/keyvault-secret.bicep' = {
  name: 'keyVaultSecretOpenAI${deploymentSuffix}'
  dependsOn: [ keyVaultModule ]
  params: {
    keyVaultName: keyVaultModule.outputs.name
    secretName: 'openAIApiKey'
    secretValue: OpenAI_ApiKey
  }
}

// --------------------------------------------------------------------------------
// Service Plan for webapp
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
      ConnectionStrings__ApplicationInsights: logAnalyticsWorkspaceModule.outputs.webAppInsightsConnectionString
      // SQL Server connection string (uses Managed Identity by default)
      ConnectionStrings__SqlDb: '@Microsoft.KeyVault(SecretUri=${keyVaultSecretSql.outputs.secretUri})'
      // OpenAI settings
      OpenAI__Models__gpt_4o_mini__DeploymentName: 'gpt-4o-mini'
      OpenAI__Models__gpt_4o_mini__Endpoint: OpenAI_Endpoint
      OpenAI__Models__gpt_4o_mini__ApiKey: '@Microsoft.KeyVault(SecretUri=${keyVaultSecretOpenAI.outputs.secretUri})'
      OpenAI__DefaultModel: 'gpt_4o_mini'
      OpenAI__Temperature: '0.8'
    }
  }
}

// --------------------------------------------------------------------------------
output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP_NAME string = resourceGroupName
output WEB_HOST_NAME string = webSiteModule.outputs.hostName
output SQL_SERVER_NAME string = sqlModule.outputs.serverName
output SQL_DATABASE_NAME string = sqlModule.outputs.databaseName

