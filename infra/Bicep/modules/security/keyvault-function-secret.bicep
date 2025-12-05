// // --------------------------------------------------------------------------------
// // This BICEP file will create a KeyVault secret for function app authentication key
// // Test:
// //   az deployment group create -n "manual-$(Get-Date -Format 'yyyyMMdd-HHmmss')" --resource-group rg-math-dev --template-file './modules/security/keyvault-function-secret.bicep' --parameters keyVaultName=mathstormkvdev secretName=functionAppApiKey functionAppName=mathstorm-func-dev functionAppResourceGroup=rg-math-dev
// // --------------------------------------------------------------------------------
// param keyVaultName string
// param secretName string
// param functionAppName string
// param functionAppResourceGroup string = resourceGroup().name
// param enabledDate string = '${substring(utcNow(), 0, 4)}-01-01T00:00:00Z'  // January 1st of current year
// param expirationDate string = '${string(int(substring(utcNow(), 0, 4)) + 1)}-12-31T23:59:59Z'  // December 31st of next year

// // --------------------------------------------------------------------------------
// resource functionApp 'Microsoft.Web/sites@2024-11-01' existing = {
//   name: functionAppName
//   scope: resourceGroup(functionAppResourceGroup)
// }

// //var functionAppKey = functionApp.listKeys().masterKey  // (Code: BadRequest)
// //var functionAppKey = functionApp.listkeys('${functionApp.id}/host/default').masterKey  // (Code: BadRequest)
// //var functionAppKey = functionApp.listKeys().keys[0].masterKey  // (Code: BadRequest)
// //var functionAppKey = functionApp.listKeys().keys[0].value  // (Code: BadRequest)
// //var functionAppKey = functionApp.listKeys('${functionApp.id}/host/default', functionApp.apiVersion).functionKeys.default
// //var functionAppKey = functionApp.listKeys().functionKeys.default
// var functionAppKey = 'unfathomable' // I wish I could figure out how to retrieve the function app key!!

// // --------------------------------------------------------------------------------
// resource keyVaultResource 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
//   name: keyVaultName
// }

// @onlyIfNotExists()
// resource createSecretValue 'Microsoft.KeyVault/vaults/secrets@2021-04-01-preview' = {
//   name: secretName
//   parent: keyVaultResource
//   properties: {
//     value: functionAppKey // secretValue // 
//     attributes: {
//       exp: dateTimeToEpoch(expirationDate)
//       nbf: dateTimeToEpoch(enabledDate)
//     }
//   }
// }

// output message string = 'Added secret ${secretName}!'
// output secretUri string = createSecretValue.properties.secretUri
// output secretName string = secretName
