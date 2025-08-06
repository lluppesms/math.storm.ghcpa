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
param functionHostName string = ''  // functionModule.outputs.hostname

param managedIdentityId string = ''
param managedIdentityPrincipalId string = ''

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
    managedIdentityId: managedIdentityId
    managedIdentityPrincipalId: managedIdentityPrincipalId
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
      FunctionService__BaseUrl: 'https://${functionHostName}/api'
      ConnectionStrings__ApplicationInsights: logAnalyticsWorkspaceModule.outputs.appInsightsConnectionString
    }
  }
}

// --------------------------------------------------------------------------------
output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP_NAME string = resourceGroupName
output WEB_HOST_NAME string = webSiteModule.outputs.hostName
