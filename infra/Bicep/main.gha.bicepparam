// ----------------------------------------------------------------------------------------------------
// Bicep Parameter File
// ----------------------------------------------------------------------------------------------------

using './main.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{ENVCODE}#'
param location = '#{RESOURCEGROUP_LOCATION}#'

// param servicePlanName = '#{servicePlanName}#'
// param webSiteSku = '#{webSiteSku}#'

param deduplicateKeyVaultSecrets = false
param principalId = '#{PRINCIPALID}#'
param myIpAddress = '#{MYIPADDRESS}#'

