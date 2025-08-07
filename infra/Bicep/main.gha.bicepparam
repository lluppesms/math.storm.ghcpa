// ----------------------------------------------------------------------------------------------------
// Bicep Parameter File
// ----------------------------------------------------------------------------------------------------

using './main.bicep'

param appName = readEnvironmentVariable('APP_NAME')
param environmentCode =  readEnvironmentVariable('ENVCODE')
param location = readEnvironmentVariable('RESOURCEGROUP_LOCATION')

param principalId = readEnvironmentVariable('PRINCIPAL_ID')
param myIpAddress = readEnvironmentVariable('MY_IP_ADDRESS')
param deployCosmos = readEnvironmentVariable('deployCosmos')

param deduplicateKeyVaultSecrets = false

// param appName = '#{APP_NAME}#'
// param environmentCode = '#{ENVCODE}#'
// param location = '#{RESOURCEGROUP_LOCATION}#'

// param deduplicateKeyVaultSecrets = false
// param principalId = '#{PRINCIPALID}#'
// param myIpAddress = '#{MYIPADDRESS}#'
// param deployCosmos = #{deployCosmos}#

// param servicePlanName = '#{servicePlanName}#'
// param webSiteSku = '#{webSiteSku}#'
// param webAppKind string = '#{webAppKind}#'
// param storageSku string = '#{storageSku}#'
// param functionAppSku string = '#{functionAppSku}#'
// param functionAppSkuFamily string = '#{functionAppSkuFamily}#'
// param functionAppSkuTier string = '#{functionAppSkuTier}#'

