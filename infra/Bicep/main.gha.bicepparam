// ----------------------------------------------------------------------------------------------------
// Bicep Parameter File
// Experiment: change this to read environment instead of using Quezta replace
// See: https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/bicep-functions-parameters-file
// ----------------------------------------------------------------------------------------------------
// Conclusion: it works, but you will have to hard-code SET the environment variables in the 
// GitHub Actions workflow, so that makes your template unique and non-reusable...
// ----------------------------------------------------------------------------------------------------

using './main.bicep'

param appName = readEnvironmentVariable('APP_NAME')
param environmentCode =  readEnvironmentVariable('ENVIRONMENTCODE')
param location = readEnvironmentVariable('LOCATION')

param principalId = readEnvironmentVariable('PRINCIPALID')
param myIpAddress = readEnvironmentVariable('MYIPADDRESS')
param deployCosmos = readEnvironmentVariable('deployCosmos') == 'true'

param deduplicateKeyVaultSecrets = false

// If you use Quetza, you can use the following parameters and it's totally reusable
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

