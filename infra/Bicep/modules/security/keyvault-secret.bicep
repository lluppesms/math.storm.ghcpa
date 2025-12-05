// --------------------------------------------------------------------------------
// This BICEP file will create a KeyVault secret
//   if existingSecretNames list is supplied: 
//     ONLY create if secretName is not in existingSecretNames list
//     OR forceSecretCreation is true
// --------------------------------------------------------------------------------
param keyVaultName string
param secretName string
param tags object = {}
param contentType string = 'string'
@description('The value of the secret. Provide only derived values like blob storage access, but do not hard code any secrets in your templates')
@secure()
param secretValue string
param enabledDate string = '${substring(utcNow(), 0, 4)}-01-01T00:00:00Z'  // January 1st of current year
param expirationDate string = '${string(int(substring(utcNow(), 0, 4)) + 1)}-12-31T23:59:59Z'  // December 31st of next year
param enabled bool = true

// --------------------------------------------------------------------------------
resource keyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

@onlyIfNotExists()
resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: secretName
  tags: tags
  parent: keyVaultResource
  properties: {
    attributes: {
      enabled: enabled
      exp: dateTimeToEpoch(expirationDate)
      nbf: dateTimeToEpoch(enabledDate)
    }
    contentType: contentType
    value: secretValue
  }
}


output message string = 'Added secret ${secretName}!'
output secretUri string = keyVaultSecret.properties.secretUri
output secretName string = secretName
