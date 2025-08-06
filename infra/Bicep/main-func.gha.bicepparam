// ----------------------------------------------------------------------------------------------------
// Bicep Parameter File
// ----------------------------------------------------------------------------------------------------

using './main-func.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{ENVCODE}#'
param location = '#{RESOURCEGROUP_LOCATION}#'

param deduplicateKeyVaultSecrets = false
param principalId = '#{PRINCIPALID}#'
param myIpAddress = '#{MYIPADDRESS}#'
param deployCosmos = #{deployCosmos}#
