// --------------------------------------------------------------------------------
// Main Bicep file that creates all of the Azure Resources for one environment
// --------------------------------------------------------------------------------
// To deploy this Bicep manually:
// 	 az login
//   az account set --subscription <subscriptionId>
//   az deployment group create -n "manual-$(Get-Date -Format 'yyyyMMdd-HHmmss')" --resource-group rg_dadabase_test --template-file 'main.bicep' --parameters appName=xxx-dadabase-test environmentCode=demo keyVaultOwnerUserId=xxxxxxxx-xxxx-xxxx
// --------------------------------------------------------------------------------
param appName string = ''
param environmentCode string = 'azd'
param location string = resourceGroup().location

param storageSku string = 'Standard_LRS'
param webSiteSku string = 'B1'

param apiKey string = ''

param adInstance string = environment().authentication.loginEndpoint // 'https://login.microsoftonline.com/'
param adDomain string = ''
param adTenantId string = ''
param adClientId string = ''
param adCallbackPath string = '/signin-oidc'

param appDataSource string = 'JSON'
param appSwaggerEnabled string = 'true'
param servicePlanName string = ''
param webAppKind string = 'linux' // 'linux' or 'windows'

// @description('Admin IP Address to add to Key Vault and Container Registry?')
// param myIpAddress string = ''
// @description('Add Role Assignments for the user assigned identity?')
// param addRoleAssignments bool = true
// // --------------------------------------------------------------------------------------------------------------
// // You may supply an existing Container Registry
// // --------------------------------------------------------------------------------------------------------------
// @description('Name of an existing Container Registry to use')
// param existing_ACR_Name string = ''
// @description('Name of ResourceGroup for an existing Container Registry')
// param existing_ACR_ResourceGroupName string = ''
// param deployContainerAppEnvironment bool = false

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
module logAnalyticsWorkspaceModule 'loganalyticsworkspace.bicep' = {
  name: 'logAnalytics${deploymentSuffix}'
  params: {
    logAnalyticsWorkspaceName: resourceNames.outputs.logAnalyticsWorkspaceName
    location: location
    commonTags: commonTags
  }
}

// --------------------------------------------------------------------------------
module storageModule 'storageaccount.bicep' = {
  name: 'storage${deploymentSuffix}'
  params: {
    storageSku: storageSku
    storageAccountName: resourceNames.outputs.storageAccountName
    location: location
    commonTags: commonTags
  }
}

module appServicePlanModule 'websiteserviceplan.bicep' = {
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


module webSiteModule 'website.bicep' = {
  name: 'webSite${deploymentSuffix}'
  params: {
    webSiteName: resourceNames.outputs.webSiteName
    location: location
    appInsightsLocation: location
    commonTags: commonTags
    environmentCode: environmentCode
    webAppKind: webAppKind
    workspaceId: logAnalyticsWorkspaceModule.outputs.id
    appServicePlanName: appServicePlanModule.outputs.name
  }
}

// In a Linux app service, any nested JSON app key like AppSettings:MyKey needs to be 
// configured in App Service as AppSettings__MyKey for the key name. 
// In other words, any : should be replaced by __ (double underscore).
// NOTE: See https://learn.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal  
module webSiteAppSettingsModule 'websiteappsettings.bicep' = {
  name: 'webSiteAppSettings${deploymentSuffix}'
  params: {
    webAppName: webSiteModule.outputs.name
    appInsightsKey: webSiteModule.outputs.appInsightsKey
    customAppSettings: {
      AppSettings__AppInsights_InstrumentationKey: webSiteModule.outputs.appInsightsKey
      AppSettings__EnvironmentName: environmentCode
      AppSettings__EnableSwagger: appSwaggerEnabled
      AppSettings__DataSource: appDataSource
      AppSettings__ApiKey: apiKey
      AzureAD__Instance: adInstance
      AzureAD__Domain: adDomain
      AzureAD__TenantId: adTenantId
      AzureAD__ClientId: adClientId
      AzureAD__CallbackPath: adCallbackPath
    }
  }
}

// module identity 'identity.bicep' = if (deployContainerAppEnvironment) {
//   name: 'app-identity${deploymentSuffix}'
//   params: {
//     identityName: resourceNames.outputs.userAssignedIdentityName
//     location: location
//   }
// }
// module roleAssignments 'role-assignments.bicep' = if (deployContainerAppEnvironment && addRoleAssignments) {
//   name: 'identity-access${deploymentSuffix}'
//   params: {
//     registryName: containerRegistry.outputs.name
//     storageAccountName: storageModule.outputs.name
//     identityPrincipalId: identity.outputs.managedIdentityPrincipalId
//   }
// }

// // --------------------------------------------------------------------------------------------------------------
// // -- Container Registry ----------------------------------------------------------------------------------------
// // --------------------------------------------------------------------------------------------------------------
// module containerRegistry 'containerRegistry.bicep' = if (deployContainerAppEnvironment) {
//   name: 'containerregistry${deploymentSuffix}'
//   params: {
//     existingRegistryName: existing_ACR_Name
//     existing_ACR_ResourceGroupName: existing_ACR_ResourceGroupName
//     newRegistryName: resourceNames.outputs.containerRegistryName
//     location: location
//     acrSku: 'Premium'
//     tags: commonTags
//     publicAccessEnabled: true // publicAccessEnabled
//     myIpAddress: myIpAddress
//   }
// }
// // --------------------------------------------------------------------------------------------------------------
// // -- Container App Environment ---------------------------------------------------------------------------------
// // --------------------------------------------------------------------------------------------------------------
// module managedEnvironment 'containerAppEnvironment.bicep' = if (deployContainerAppEnvironment) {
//   name: 'cae${deploymentSuffix}'
//   params: {
//     newEnvironmentName: resourceNames.outputs.caManagedEnvName
//     location: location
//     logAnalyticsWorkspaceName: logAnalyticsWorkspaceModule.outputs.name
//     logAnalyticsRgName: resourceGroupName
//     tags: commonTags
//     publicAccessEnabled: true // publicAccessEnabled
//   }
// }

// // --------------------------------------------------------------------------------------------------------------
// // -- UI Application Definition ---------------------------------------------------------------------------------
// // --------------------------------------------------------------------------------------------------------------
// module app 'containerApp.bicep' = if (deployContainerAppEnvironment) {
//   name: 'ui-app${deploymentSuffix}'
//   params: {
//     name: resourceNames.outputs.containerAppUIName
//     location: location
//     tags: commonTags
//     applicationInsightsName: webSiteModule.outputs.appInsightsName
//     managedEnvironmentName: managedEnvironment.outputs.name
//     managedEnvironmentRg: managedEnvironment.outputs.resourceGroupName
//     containerRegistryName: containerRegistry.outputs.name
//     imageName: resourceNames.outputs.containerAppUIName
//     exists: false
//     identityName: identity.outputs.managedIdentityName
//     deploymentSuffix: deploymentSuffix
//     env: [
//       { name: 'AzureStorageAccountEndPoint', value: 'https://${storageModule.outputs.name}.blob.${environment().suffixes.storage}' }
//       { name: 'AzureStorageUserUploadContainer', value: 'content' }
//       { name: 'UserAssignedManagedIdentityClientId', value: identity.outputs.managedIdentityClientId }
//       { name: 'AZURE_CLIENT_ID', value: identity.outputs.managedIdentityClientId }
//     ]
//     port: 8080
//     secrets: {}
//     // secrets: {
//     //   cosmos: 'https://${keyVault.outputs.name}${environment().suffixes.keyvaultDns}/secrets/${cosmos.outputs.connectionStringSecretName}'
//     //   aikey: 'https://${keyVault.outputs.name}${environment().suffixes.keyvaultDns}/secrets/${azureOpenAi.outputs.cognitiveServicesKeySecretName}'
//     //   searchkey: 'https://${keyVault.outputs.name}${environment().suffixes.keyvaultDns}/secrets/${searchService.outputs.searchKeySecretName}'
//     //   docintellikey: 'https://${keyVault.outputs.name}${environment().suffixes.keyvaultDns}/secrets/${documentIntelligence.outputs.keyVaultSecretName}'
//     //   apikey: 'https://${keyVault.outputs.name}${environment().suffixes.keyvaultDns}/secrets/api-key'
//     // }
//   }
// }
//output ACA_HOST_NAME string = deployContainerAppEnvironment ? app.outputs.hostName : ''

output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP_NAME string = resourceGroupName
output HOST_NAME string = webSiteModule.outputs.hostName
