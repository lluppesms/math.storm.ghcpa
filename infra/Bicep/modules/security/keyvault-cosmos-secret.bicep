// --------------------------------------------------------------------------------
// This BICEP file will create a KeyVault secret for a Cosmos connection
//   if existingSecretNames list is supplied:
//     ONLY create if secretName is not in existingSecretNames list
//     OR forceSecretCreation is true
// --------------------------------------------------------------------------------
param keyVaultName string
param accountKeySecretName string
param connectionStringSecretName string
param cosmosAccountName string
param cosmosAccountResourceGroup string = resourceGroup().name
param enabledDate string = '${substring(utcNow(), 0, 4)}-01-01T00:00:00Z'  // January 1st of current year
param expirationDate string = '${string(int(substring(utcNow(), 0, 4)) + 1)}-12-31T23:59:59Z'  // December 31st of next year

// --------------------------------------------------------------------------------
resource cosmosResource 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' existing = {
   name: cosmosAccountName
  scope: resourceGroup(cosmosAccountResourceGroup)
}
var cosmosKey = cosmosResource.listKeys().primaryMasterKey
var cosmosConnectionString = 'AccountEndpoint=https://${cosmosAccountName}.documents.azure.com:443/;AccountKey=${cosmosKey}'

resource keyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

@onlyIfNotExists()
resource createSecretKeyValue 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(accountKeySecretName)) {
  name: accountKeySecretName
  parent: keyVaultResource
  properties: {
    value: cosmosKey
    attributes: {
      exp: dateTimeToEpoch(expirationDate)
      nbf: dateTimeToEpoch(enabledDate)
    }
  }
}

@onlyIfNotExists()
resource createSecretConnectionValue 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(connectionStringSecretName)) {
  name: connectionStringSecretName
  parent: keyVaultResource
  properties: {
    value: cosmosConnectionString
    attributes: {
      exp: dateTimeToEpoch(expirationDate)
      nbf: dateTimeToEpoch(enabledDate)
    }
  }
}

output message string = 'Added secret ${accountKeySecretName} and/or ${connectionStringSecretName}!'

output accountKeySecretUri string = !empty(accountKeySecretName) ? createSecretKeyValue.properties.secretUri : ''
output accountKeySecretName string = !empty(accountKeySecretName) ? accountKeySecretName : ''

output connectionStringSecretUri string = !empty(connectionStringSecretName) ? createSecretConnectionValue.properties.secretUri : ''
output connectionStringSecretName string = !empty(connectionStringSecretName) ? connectionStringSecretName : ''
