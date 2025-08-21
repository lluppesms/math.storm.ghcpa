// --------------------------------------------------------------------------------
// This BICEP file will retrieve the function app host key for authentication
// --------------------------------------------------------------------------------
param functionAppName string
param functionAppResourceGroup string = resourceGroup().name

// --------------------------------------------------------------------------------
resource functionApp 'Microsoft.Web/sites@2023-01-01' existing = {
  name: functionAppName
  scope: resourceGroup(functionAppResourceGroup)
}

// --------------------------------------------------------------------------------
output functionAppName string = functionAppName
output functionMasterKey string = functionApp.listKeys().masterKey
output functionDefaultKey string = functionApp.listKeys().functionKeys.default