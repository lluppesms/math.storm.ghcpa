// --------------------------------------------------------------------------------
// Bicep file that builds all the resource names used by other Bicep templates
// --------------------------------------------------------------------------------
param appName string = ''
// @allowed(['azd','gha','azdo','dev','demo','qa','stg','ct','prod'])
param environmentCode string = 'azd'
param environmentSpecificFunctionName string = ''
param functionStorageNameSuffix string = 'func'
param dataStorageNameSuffix string = 'data'

// --------------------------------------------------------------------------------
var sanitizedEnvironment = toLower(environmentCode)
var lowerAppName = replace(toLower(appName), ' ', '')
var sanitizedAppNameWithDashes = replace(replace(toLower(appName), ' ', ''), '_', '')
var sanitizedAppName = replace(replace(replace(toLower(appName), ' ', ''), '-', ''), '_', '')

// pull resource abbreviations from a common JSON file
var resourceAbbreviations = loadJsonContent('./data/abbreviation.json')

// --------------------------------------------------------------------------------
var webSiteName = environmentCode == 'prod' ? toLower(sanitizedAppNameWithDashes) : toLower('${sanitizedAppNameWithDashes}-${sanitizedEnvironment}')
output webSiteName string                = webSiteName
output webSiteAppServicePlanName string  = '${webSiteName}-${resourceAbbreviations.webServerFarms}'
output webSiteAppInsightsName string     = '${webSiteName}-${resourceAbbreviations.webSitesAppService}'

var functionAppName = environmentSpecificFunctionName == '' ? environmentCode == 'azd' ? '${lowerAppName}function' : toLower('${lowerAppName}-func-${sanitizedEnvironment}') : environmentSpecificFunctionName
output functionAppName string            = functionAppName
output functionAppServicePlanName string = '${functionAppName}-${resourceAbbreviations.webServerFarms}'
output functionAppInsightsName string    = '${functionAppName}-${resourceAbbreviations.webSitesAppService}'

output logAnalyticsWorkspaceName string  = toLower('${sanitizedAppNameWithDashes}-${sanitizedEnvironment}-${resourceAbbreviations.operationalInsightsWorkspaces}')
output cosmosDatabaseName string         = toLower('${sanitizedAppName}-${resourceAbbreviations.documentDBDatabaseAccounts}-${sanitizedEnvironment}')

output userAssignedIdentityName string   = toLower('${sanitizedAppName}-${resourceAbbreviations.managedIdentityUserAssignedIdentities}-${sanitizedEnvironment}')

// Key Vaults and Storage Accounts can only be 24 characters long
output keyVaultName string               = take('${sanitizedAppName}${resourceAbbreviations.keyVaultVaults}${sanitizedEnvironment}', 24)
output storageAccountName string         = take('${sanitizedAppName}${resourceAbbreviations.storageStorageAccounts}${sanitizedEnvironment}${dataStorageNameSuffix}', 24)
output functionStorageName string        = take('${sanitizedAppName}${resourceAbbreviations.storageStorageAccounts}${sanitizedEnvironment}${functionStorageNameSuffix}', 24)
