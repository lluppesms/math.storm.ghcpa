// --------------------------------------------------------------------------------
// This BICEP file will create KeyVault secret for a storage account connection
//   if existingSecretNames list is supplied: 
//     ONLY create if secretName is not in existingSecretNames list
//     OR forceSecretCreation is true
// --------------------------------------------------------------------------------
param keyVaultName string
param secretName string
param storageAccountName string
param storageAccountResourceGroup string = resourceGroup().name
param enabledDate string = '${substring(utcNow(), 0, 4)}-01-01T00:00:00Z'  // January 1st of current year
param expirationDate string = '${string(int(substring(utcNow(), 0, 4)) + 1)}-12-31T23:59:59Z'  // December 31st of next year

// --------------------------------------------------------------------------------
resource storageAccountResource 'Microsoft.Storage/storageAccounts@2021-04-01' existing = { 
  name: storageAccountName 
  scope: resourceGroup(storageAccountResourceGroup)
}
var accountKey = storageAccountResource.listKeys().keys[0].value
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountResource.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${accountKey}'

// --------------------------------------------------------------------------------
resource keyVaultResource 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

resource createSecretValue 'Microsoft.KeyVault/vaults/secrets@2021-04-01-preview' = {
  name: secretName
  parent: keyVaultResource
  properties: {
    value: storageAccountConnectionString
    attributes: {
      exp: dateTimeToEpoch(expirationDate)
      nbf: dateTimeToEpoch(enabledDate)
    }
  }
}

output message string = 'Added secret ${secretName}!'
output secretUri string = createSecretValue.properties.secretUri
output secretName string = secretName
