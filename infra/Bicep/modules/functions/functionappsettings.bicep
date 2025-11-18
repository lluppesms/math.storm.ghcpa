// --------------------------------------------------------------------------------
// This BICEP file will add unique Configuration settings to a web or function app
// --------------------------------------------------------------------------------
param functionAppName string
param functionStorageAccountName string
param functionInsightsKey string
param customAppSettings object = {}
param functionsWorkerRuntime string = 'DOTNET-ISOLATED'
param functionsExtensionVersion string = '~4'
param use32BitProcess string = 'false'
param netFrameworkVersion string = 'v10.0'
param usePlaceholderDotNetIsolated string = '1'

//param keyVaultName string
//param nodeDefaultVersion string = '8.11.1'

param cosmosAccountName string = ''

param OpenAI_Gpt4o_DeploymentName string = 'gpt-4o-mini'
param OpenAI_Gpt4o_Endpoint string = ''
param OpenAI_Gpt4o_ApiKey string = ''
param OpenAI_Gpt35_DeploymentName string = 'gpt-35-turbo'
param OpenAI_Gpt35_Endpoint string = ''
param OpenAI_Gpt35_ApiKey string = ''


// --------------------------------------------------------------------------------
//var useKeyVaultConnection = false

var addCosmos = !empty(cosmosAccountName)
var addOpenAI = !empty(OpenAI_Gpt4o_Endpoint) && !empty(OpenAI_Gpt35_Endpoint)

// --------------------------------------------------------------------------------
// resource storageAccountResource 'Microsoft.Storage/storageAccounts@2019-06-01' existing = { 
//   name: functionStorageAccountName 
// }

resource cosmosResource 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' existing = if (addCosmos) {
   name: cosmosAccountName
}

//var accountKey = storageAccountResource.listKeys().keys[0].value
// var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountResource.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${accountKey}'

// var functionStorageAccountKeyVaultReference = '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azurefilesconnectionstring)'

var cosmosKey = cosmosResource.listKeys().primaryMasterKey
var cosmosConnectionString = 'AccountEndpoint=https://${cosmosAccountName}.documents.azure.com:443/;AccountKey=${cosmosKey}'


var cosmosSettings = addCosmos ? {
  CosmosDb__ConnectionString: cosmosConnectionString
  CosmosDb__Endpoint__Save: 'https://${cosmosAccountName}.documents.azure.com:443/'
} : {}

var openAISettings = addOpenAI ? {
  OpenAI__Models__gpt_4o_mini__DeploymentName: OpenAI_Gpt4o_DeploymentName
  OpenAI__Models__gpt_4o_mini__Endpoint: OpenAI_Gpt4o_Endpoint
  OpenAI__Models__gpt_4o_mini__ApiKey: OpenAI_Gpt4o_ApiKey

  OpenAI__Models__gpt_35_turbo__DeploymentName: OpenAI_Gpt35_DeploymentName
  OpenAI__Models__gpt_35_turbo__Endpoint: OpenAI_Gpt35_Endpoint
  OpenAI__Models__gpt_35_turbo__ApiKey: OpenAI_Gpt35_ApiKey
} : {}

var BASE_SLOT_APPSETTINGS = {
  // See https://learn.microsoft.com/en-us/azure/azure-functions/functions-identity-based-connections-tutorial
  APPLICATIONINSIGHTS_CONNECTION_STRING: 'InstrumentationKey=${functionInsightsKey}'
  APPINSIGHTS_INSTRUMENTATIONKEY: functionInsightsKey
  AzureWebJobsStorage__accountName: functionStorageAccountName
  AzureWebJobsSecretStorageType: 'files'
  FUNCTIONS_WORKER_RUNTIME: functionsWorkerRuntime
  FUNCTIONS_EXTENSION_VERSION: functionsExtensionVersion
  USE32BITWORKERPROCESS: use32BitProcess
  NET_FRAMEWORK_VERSION: netFrameworkVersion
  WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: usePlaceholderDotNetIsolated
  WEBSITE_RUN_FROM_PACKAGE: '1'
  
  //AzureWebJobsDashboard: useKeyVaultConnection ? functionStorageAccountKeyVaultReference : storageAccountConnectionString
  //AzureWebJobsStorage: useKeyVaultConnection ? functionStorageAccountKeyVaultReference : storageAccountConnectionString
  // WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: useKeyVaultConnection ? functionStorageAccountKeyVaultReference : storageAccountConnectionString
  // WEBSITE_CONTENTSHARE: functionAppName
  // WEBSITE_NODE_DEFAULT_VERSION: nodeDefaultVersion
}

// This *should* work, but I keep getting a "circular dependency detected" error and it doesn't work
// resource appResource 'Microsoft.Web/sites@2021-03-01' existing = { name: functionAppName }
// var BASE_SLOT_APPSETTINGS = list('${appResource.id}/config/appsettings', appResource.apiVersion).properties

resource functionApp 'Microsoft.Web/sites@2021-02-01' existing = {
  name: functionAppName
}

resource siteConfig 'Microsoft.Web/sites/config@2021-02-01' = {
  name: 'appsettings'
  parent: functionApp
  properties: union(BASE_SLOT_APPSETTINGS, customAppSettings, cosmosSettings, openAISettings)
}
