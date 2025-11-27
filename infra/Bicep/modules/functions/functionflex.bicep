// ----------------------------------------------------------------------------------------------------
// This BICEP file will create an .NET 10 Isolated Azure Function
// See: https://github.com/Azure-Samples/azure-functions-flex-consumption-samples/blob/main/IaC/bicep/main.bicep
// ----------------------------------------------------------------------------------------------------
param functionAppName string = 'll-flex-test-2'
param functionAppServicePlanName string
param functionInsightsName string
param functionStorageAccountName string
@allowed([ 'functionapp', 'functionapp,linux' ])
param functionKind string = 'functionapp,linux'
param runtimeName string = 'dotnet-isolated'
param runtimeVersion string = '10.0'
@minValue(10)
@maxValue(1000)
param maximumInstanceCount int = 50
@allowed([512,2048,4096])
param instanceMemoryMB int = 2048

param location string = resourceGroup().location
param commonTags object = {}
@description('The workspace to store audit logs.')
param workspaceId string = ''
@description('Id of the user running this template, to be used for testing and debugging for access to Azure resources. This is not required in production. Leave empty if not needed.')
param adminPrincipalId string = ''
param deploymentSuffix string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~functionapp.bicep' }
var azdTag = { 'azd-service-name': 'function' }
var tags = union(commonTags, templateTag)
var functionTags = union(commonTags, templateTag, azdTag)
var resourceToken = toLower(uniqueString(subscription().id, resourceGroup().name, location))
var deploymentStorageContainerName = 'app-package-${take(functionAppName, 32)}-${take(resourceToken, 7)}'

// --------------------------------------------------------------------------------
module applicationInsights 'br/public:avm/res/insights/component:0.6.0' = {
  name: 'flexappinsights${deploymentSuffix}'
  params: {
    name: functionInsightsName
    location: location
    tags: tags
    workspaceResourceId: workspaceId
    disableLocalAuth: true
  }
}

// Backing storage for Azure Functions
module storageAccountResource 'br/public:avm/res/storage/storage-account:0.25.0' = {
  name: 'flexstorage${deploymentSuffix}'
  params: {
    name: functionStorageAccountName
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false // Disable local authentication methods as per policy
    dnsEndpointType: 'Standard'
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    blobServices: {
      containers: [{name: deploymentStorageContainerName}]
    }
    tableServices:{}
    queueServices: {}
    minimumTlsVersion: 'TLS1_2'  // Enforcing TLS 1.2 for better security
    location: location
    tags: tags
  }
}

// Create an App Service Plan to group applications under the same payment plan and SKU
module appServiceResource 'br/public:avm/res/web/serverfarm:0.1.1' = {
  name: 'flexappservice${deploymentSuffix}'
  params: {
    name: functionAppServicePlanName
    sku: {
      name: 'FC1'
      tier: 'FlexConsumption'
    }
    reserved: true
    location: location
    tags: functionTags
  }
}

// --------------------------------------------------------------------------------
//resource functionAppResource 'Microsoft.Web/sites@2024-11-01' = {
module functionAppResource 'br/public:avm/res/web/site:0.16.0' = {
  name: 'flexapp${deploymentSuffix}'
  params: {
    name: functionAppName
    location: location
    kind: functionKind
    tags: functionTags
    managedIdentities: {
      systemAssigned: true
    }
    serverFarmResourceId: appServiceResource.outputs.resourceId
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageAccountResource.outputs.primaryBlobEndpoint}${deploymentStorageContainerName}'
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }
      runtime: { 
        name: runtimeName
        version: runtimeVersion
      }
    }
    siteConfig: {
      alwaysOn: false
    }
    configs: [{
      name: 'appsettings'
      properties:{
        // Only include required credential settings unconditionally
        AzureWebJobsStorage__credential: 'managedidentity'
        AzureWebJobsStorage__blobServiceUri: 'https://${storageAccountResource.outputs.name}.blob.${environment().suffixes.storage}'
        AzureWebJobsStorage__queueServiceUri: 'https://${storageAccountResource.outputs.name}.queue.${environment().suffixes.storage}'
        AzureWebJobsStorage__tableServiceUri: 'https://${storageAccountResource.outputs.name}.table.${environment().suffixes.storage}'

        // Application Insights settings are always included
        APPLICATIONINSIGHTS_CONNECTION_STRING: applicationInsights.outputs.connectionString
        APPLICATIONINSIGHTS_AUTHENTICATION_STRING: 'Authorization=AAD'
    }
    }]
  }
}

// Consolidated Role Assignments
module rbacAssignments './functionflexrbac.bicep' = {
  name: 'flexrbac${deploymentSuffix}'
  params: {
    storageAccountName: storageAccountResource.outputs.name
    appInsightsName: applicationInsights.outputs.name
    managedIdentityPrincipalId: functionAppResource.outputs.?systemAssignedMIPrincipalId ?? ''
    //userIdentityPrincipalId: adminPrincipalId
    //allowUserIdentityPrincipal: !empty(adminPrincipalId)
  }
}

// --------------------------------------------------------------------------------
output id string = functionAppResource.outputs.resourceId
output hostname string = functionAppResource.outputs.defaultHostname
output name string = functionAppName
output insightsName string = functionInsightsName
output insightsKey string = applicationInsights.outputs.instrumentationKey
output storageAccountName string = functionStorageAccountName
output functionAppPrincipalId string = functionAppResource.outputs.?systemAssignedMIPrincipalId ?? ''
