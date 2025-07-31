// ----------------------------------------------------------------------------------------------------
// Bicep Parameter File
// ----------------------------------------------------------------------------------------------------

using './main.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{ENVCODE}#'
param location = '#{RESOURCEGROUP_LOCATION}#'

param deduplicateKeyVaultSecrets = false
param principalId = '#{PRINCIPALID}#'
param myIpAddress = '#{MYIPADDRESS}#'
param deployCosmos = #{deployCosmos}#

// param servicePlanName = '#{servicePlanName}#'
// param webSiteSku = '#{webSiteSku}#'
// param webAppKind string = '#{webAppKind}#'
// param storageSku string = '#{storageSku}#'
// param functionAppSku string = '#{functionAppSku}#'
// param functionAppSkuFamily string = '#{functionAppSkuFamily}#'
// param functionAppSkuTier string = '#{functionAppSkuTier}#'

