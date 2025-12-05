// --------------------------------------------------------------------------------
// This BICEP file will create KeyVault Password secret for an existing Search Service
//   if existingSecretNames list is supplied: 
//     ONLY create if secretName is not in existingSecretNames list
//     OR forceSecretCreation is true
// --------------------------------------------------------------------------------
param keyVaultName string
param secretName string
param searchServiceName string
param searchServiceResourceGroup string
param enabledDate string = '${substring(utcNow(), 0, 4)}-01-01T00:00:00Z'  // January 1st of current year
param expirationDate string = '${string(int(substring(utcNow(), 0, 4)) + 1)}-12-31T23:59:59Z'  // December 31st of next year
param enabled bool = true

// --------------------------------------------------------------------------------
resource existingResource 'Microsoft.Search/searchServices@2023-11-01' existing = {
  scope: resourceGroup(searchServiceResourceGroup)
  name: searchServiceName
}
var secretValue = existingResource.listAdminKeys().primaryKey

resource keyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

@onlyIfNotExists()
resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: secretName
  parent: keyVaultResource
  properties: {
    attributes: {
      enabled: enabled
      exp: dateTimeToEpoch(expirationDate)
      nbf: dateTimeToEpoch(enabledDate)
    }
    contentType: 'string'
    value: secretValue
  }
}

output message string = 'Added secret ${secretName}!'
output secretUri string = keyVaultSecret.properties.secretUri
output secretName string = secretName
