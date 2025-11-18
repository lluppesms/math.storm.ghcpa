// ----------------------------------------------------------------------------------------------------
// This BICEP file will create an .NET 8 Isolated Azure Function
// Changed to use Managed Identity Storage (no connection string)
// ----------------------------------------------------------------------------------------------------
param functionAppName string
param functionAppServicePlanName string
param functionInsightsName string
param sharedAppServicePlanName string
param sharedAppInsightsInstrumentationKey string
param sharedAppInsightsConnectionString string
param functionStorageAccountName string

param location string = resourceGroup().location
param commonTags object = {}

param managedIdentityId string
param managedIdentityPrincipalId string
// param keyVaultName string

@allowed([ 'functionapp', 'functionapp,linux' ])
param functionKind string = 'functionapp,linux'
param functionHostKind string = 'linux'
param functionAppSku string = 'Y1'
param functionAppSkuFamily string = 'Y'
param functionAppSkuTier string = 'Dynamic'
param linuxFxVersion string = 'DOTNET-ISOLATED|10.0'

param functionsWorkerRuntime string = 'DOTNET-ISOLATED'
param functionsExtensionVersion string = '~4'
//param nodeDefaultVersion string = '8.11.1'
param use32BitProcess string = 'false'
param netFrameworkVersion string = 'v10.0'
param usePlaceholderDotNetIsolated string = '1'

param workerSizeId int = 0
param numberOfWorkers int = 1
param maximumWorkerCount int = 1

param publicNetworkAccess string = 'Enabled'

@description('The workspace to store audit logs.')
param workspaceId string = ''

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~functionapp.bicep' }
var azdTag = { 'azd-service-name': 'function' }
var tags = union(commonTags, templateTag)
var functionTags = union(commonTags, templateTag, azdTag)
var useExistingServicePlan = !empty(sharedAppServicePlanName)
//var useKeyVaultConnection = false

// --------------------------------------------------------------------------------
// resource storageAccountResource 'Microsoft.Storage/storageAccounts@2019-06-01' existing = { name: functionStorageAccountName }
// var accountKey = storageAccountResource.listKeys().keys[0].value
// var functionStorageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountResource.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${accountKey}'
// var functionStorageAccountKeyVaultReference = '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azurefilesconnectionstring)'

// Use the existing shared App Service Plan
resource sharedAppServiceResource 'Microsoft.Web/serverfarms@2024-11-01' existing = if (useExistingServicePlan) {
  name: sharedAppServicePlanName
}

resource appServiceResource 'Microsoft.Web/serverfarms@2024-11-01' =  if (!useExistingServicePlan) {
  name: functionAppServicePlanName
  location: location
  kind: functionHostKind
  tags: tags
  sku: {
    name: functionAppSku
    tier: functionAppSkuTier
    size: functionAppSku
    family: functionAppSkuFamily
    capacity: 0
  }
  properties: {
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: maximumWorkerCount
    targetWorkerCount: workerSizeId
    targetWorkerSizeId: numberOfWorkers
    isSpot: false
    reserved: true
    isXenon: false
    hyperV: false
    zoneRedundant: false
  }
}

resource functionAppResource 'Microsoft.Web/sites@2024-11-01' = {
  name: functionAppName
  location: location
  kind: functionKind
  tags: functionTags
  // identity: {
  //   type: 'UserAssigned'
  //   userAssignedIdentities: { '${managedIdentityId}': {} }
  // }
  identity: {
    type: 'SystemAssigned'
  }
  // identity: {
  //   //disable-next-line BCP036
  //   type: 'SystemAssigned, UserAssigned'
  //   //disable-next-line BCP036
  //   userAssignedIdentities: { '${managedIdentityId}': {} }
  // }
  properties: {
    enabled: true
    serverFarmId: (useExistingServicePlan ? sharedAppServiceResource.id : appServiceResource.id)
    reserved: true
    isXenon: false
    hyperV: false
    vnetRouteAllEnabled: false
    vnetImagePullEnabled: false
    vnetContentShareEnabled: false
    siteConfig: {
      numberOfWorkers: numberOfWorkers
      linuxFxVersion: linuxFxVersion
      acrUseManagedIdentityCreds: false
      alwaysOn: true
      http20Enabled: false
      functionAppScaleLimit: 200
      minimumElasticInstanceCount: 0
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      appSettings: [
        // See https://learn.microsoft.com/en-us/azure/azure-functions/functions-identity-based-connections-tutorial
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: sharedAppInsightsConnectionString
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: sharedAppInsightsInstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage__accountName'
          value: functionStorageAccountName
        }
        {
          name: 'AzureWebJobsSecretStorageType'
          value: 'files'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionsWorkerRuntime
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: functionsExtensionVersion
        }
        {
          name: 'USE32BITWORKERPROCESS'
          value: use32BitProcess
        }
        {
          name: 'NET_FRAMEWORK_VERSION'
          value: netFrameworkVersion
        }
        {
          name: 'WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED'
          value: usePlaceholderDotNetIsolated
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
    scmSiteAlsoStopped: false
    clientAffinityEnabled: false
    clientCertEnabled: false
    hostNamesDisabled: false
    dailyMemoryTimeQuota: 0
    httpsOnly: true
    redundancyMode: 'None'
    publicNetworkAccess: publicNetworkAccess
    storageAccountRequired: true
    keyVaultReferenceIdentity: 'SystemAssigned'
  }
}

resource functionAppConfig 'Microsoft.Web/sites/config@2024-11-01' = {
  parent: functionAppResource
  name: 'web'
  properties: {
    numberOfWorkers: numberOfWorkers
    netFrameworkVersion: netFrameworkVersion
    linuxFxVersion: linuxFxVersion
    requestTracingEnabled: false
    remoteDebuggingEnabled: false
    httpLoggingEnabled: false
    acrUseManagedIdentityCreds: false
    logsDirectorySizeLimit: 35
    detailedErrorLoggingEnabled: false
    scmType: 'None'
    use32BitWorkerProcess: false
    webSocketsEnabled: false
    alwaysOn: true
    managedPipelineMode: 'Integrated'
    virtualApplications: [
      {
        virtualPath: '/'
        physicalPath: 'site\\wwwroot'
        preloadEnabled: false
      }
    ]
    loadBalancing: 'LeastRequests'
    experiments: {
      rampUpRules: []
    }
    autoHealEnabled: false
    vnetRouteAllEnabled: false
    vnetPrivatePortsCount: 0
    cors: {
      allowedOrigins: [
        'https://portal.azure.com'
      ]
      supportCredentials: false
    }
    localMySqlEnabled: false
    ipSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictionsUseMain: false
    http20Enabled: false
    minTlsVersion: '1.2'
    scmMinTlsVersion: '1.2'
    ftpsState: 'FtpsOnly'
    preWarmedInstanceCount: 0
    functionAppScaleLimit: 200
    functionsRuntimeScaleMonitoringEnabled: false
    minimumElasticInstanceCount: 0
    azureStorageAccounts: { }
  }
}

resource functionAppBinding 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = {
    name: '${functionAppResource.name}.azurewebsites.net'
    parent: functionAppResource
    properties: {
        siteName: functionAppName
        hostNameType: 'Verified'
    }
}

resource functionAppMetricLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${functionAppResource.name}-metrics'
  scope: functionAppResource
  properties: {
    workspaceId: workspaceId
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}
// https://learn.microsoft.com/en-us/azure/app-service/troubleshoot-diagnostic-logs
resource functionAppAuditLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${functionAppResource.name}-logs'
  scope: functionAppResource
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'FunctionAppLogs'
        enabled: true
      }
    ]
  }
}

// --------------------------------------------------------------------------------
output id string = functionAppResource.id
output hostname string = functionAppResource.properties.defaultHostName
output name string = functionAppName
output insightsName string = functionInsightsName
output insightsKey string = sharedAppInsightsInstrumentationKey
output storageAccountName string = functionStorageAccountName
output functionAppUMIPrincipalId string = managedIdentityPrincipalId
output functionAppPrincipalId string = functionAppResource.identity.principalId

// @secure() 
// output functionMasterKey string = functionAppResource.listKeys().functionKeys.default
