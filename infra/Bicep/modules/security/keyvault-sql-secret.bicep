// --------------------------------------------------------------------------------
// This BICEP file stores a SQL Server connection string as a KeyVault secret
// --------------------------------------------------------------------------------
param keyVaultName string
param secretName string
param sqlServerName string
param sqlDBName string
param sqlAdminUser string = ''
@secure()
param sqlAdminPassword string = ''
param enabledDate string = '${substring(utcNow(), 0, 4)}-01-01T00:00:00Z'
param expirationDate string = '${string(int(substring(utcNow(), 0, 4)) + 1)}-12-31T23:59:59Z'

// --------------------------------------------------------------------------------
// Build the connection string - if sqlAdminPassword is provided use SQL auth,
// otherwise use Managed Identity (Active Directory Default)
var connectionString = !empty(sqlAdminPassword)
  ? 'Server=tcp:${sqlServerName}.database.windows.net,1433;Initial Catalog=${sqlDBName};User Id=${sqlAdminUser};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;'
  : 'Server=tcp:${sqlServerName}.database.windows.net,1433;Initial Catalog=${sqlDBName};Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;Authentication="Active Directory Default";'

resource keyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource createSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: secretName
  parent: keyVaultResource
  properties: {
    value: connectionString
    attributes: {
      exp: dateTimeToEpoch(expirationDate)
      nbf: dateTimeToEpoch(enabledDate)
    }
  }
}

output secretUri string = createSecret.properties.secretUri
output secretName string = secretName
