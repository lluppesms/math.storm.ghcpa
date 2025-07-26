// ----------------------------------------------------------------------------------------------------
// Bicep Parameter File
// ----------------------------------------------------------------------------------------------------

using './main.bicep'

param appName = '#{appName}#'
param environmentCode = '#{environmentNameLower}#'
param location = '#{location}#'

// param servicePlanName = '#{servicePlanName}#'
// param webSiteSku = '#{webSiteSku}#'

param deduplicateKeyVaultSecrets = false
param principalId = '#{principalId}#'
param myIpAddress = '#{myIpAddress}#'

