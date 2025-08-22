// --------------------------------------------------------------------------------
// This BICEP file will create a KeyVault secret for function app authentication key
// Test:
//   az deployment group create -n "manual-$(Get-Date -Format 'yyyyMMdd-HHmmss')" --resource-group rg-math-dev --template-file './modules/security/keyvault-function-secret.bicep' --parameters keyVaultName=mathstormkvdev secretName=functionAppApiKey functionAppName=mathstorm-func-dev functionAppResourceGroup=rg-math-dev
// --------------------------------------------------------------------------------
param keyVaultName string
param secretName string
@description('The value of the secret. Provide only derived values like blob storage access, but do not hard code any secrets in your templates')
@secure()
param secretValue string
// param functionAppName string
// param functionAppResourceGroup string = resourceGroup().name
param enabledDate string = utcNow()
param expirationDate string = dateTimeAdd(utcNow(), 'P2Y')
param existingSecretNames string = ''
param forceSecretCreation bool = false

// --------------------------------------------------------------------------------
var secretExists = contains(toLower(existingSecretNames), ';${toLower(trim(secretName))};')

// --------------------------------------------------------------------------------
// resource functionApp 'Microsoft.Web/sites@2023-01-01' existing = {
//   name: functionAppName
//   scope: resourceGroup(functionAppResourceGroup)
// }

// //var functionAppKey = functionApp.listKeys().masterKey  // (Code: BadRequest)
// //var functionAppKey = functionApp.listkeys('${functionApp.id}/host/default').masterKey  // (Code: BadRequest)
// //var functionAppKey = functionApp.listKeys().keys[0].masterKey  // (Code: BadRequest)
// var functionAppKey = functionApp.listKeys().keys[0].value  // (Code: BadRequest)

// --------------------------------------------------------------------------------
resource keyVaultResource 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

resource createSecretValue 'Microsoft.KeyVault/vaults/secrets@2021-04-01-preview' = if (!secretExists || forceSecretCreation) {
  name: secretName
  parent: keyVaultResource
  properties: {
    value: secretValue // functionAppKey
    attributes: {
      exp: dateTimeToEpoch(expirationDate)
      nbf: dateTimeToEpoch(enabledDate)
    }
  }
}

var createMessage = secretExists ? 'Secret ${secretName} already exists!' : 'Added secret ${secretName}!'
output message string = secretExists && forceSecretCreation ? 'Secret ${secretName} already exists but was recreated!' : createMessage
output secretCreated bool = !secretExists
output secretUri string = createSecretValue.properties.secretUri
output secretName string = secretName
