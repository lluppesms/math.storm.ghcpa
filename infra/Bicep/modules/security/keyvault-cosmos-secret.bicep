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
param enabledDate string = utcNow()
param expirationDate string = dateTimeAdd(utcNow(), 'P2Y')
param existingSecretNames string = ''
param forceSecretCreation bool = false

// --------------------------------------------------------------------------------
var keySecretExists = contains(toLower(existingSecretNames), ';${toLower(trim(accountKeySecretName))};')
var connectionStringSecretExists = contains(toLower(existingSecretNames), ';${toLower(trim(connectionStringSecretName))};')

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

resource createSecretKeyValue 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if ((!keySecretExists || forceSecretCreation) && !empty(accountKeySecretName)) {
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

resource createSecretConnectionValue 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if ((!connectionStringSecretExists || forceSecretCreation) && !empty(connectionStringSecretName)) {
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

var createMessage = keySecretExists ? 'Secret ${accountKeySecretName} already exists!' : 'Added secret ${accountKeySecretName}!'
output message string = keySecretExists && forceSecretCreation ? 'Secret ${accountKeySecretName} already exists but was recreated!' : createMessage
output secretCreated bool = !keySecretExists

output accountKeySecretUri string = !empty(accountKeySecretName) ? createSecretKeyValue.properties.secretUri : ''
output accountKeySecretName string = !empty(accountKeySecretName) ? accountKeySecretName : ''

output connectionStringSecretUri string = !empty(connectionStringSecretName) ? createSecretConnectionValue.properties.secretUri : ''
output connectionStringSecretName string = !empty(connectionStringSecretName) ? connectionStringSecretName : ''
