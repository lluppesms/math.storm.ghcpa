// --------------------------------------------------------------------------------
// Bicep file that builds all the resource names used by other Bicep templates
// --------------------------------------------------------------------------------
param appName string = ''
// @allowed(['azd','gha','azdo','dev','demo','qa','stg','ct','prod'])
param environmentCode string = 'azd'

param functionStorageNameSuffix string = 'func'
param functionFlexStorageNameSuffix string = 'flex'
param environmentSpecificFunctionName string = ''
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
output webSiteAppInsightsName string     = '${webSiteName}-${resourceAbbreviations.insightsComponents}'

var functionAppName = environmentSpecificFunctionName == '' ? environmentCode == 'azd' ? '${lowerAppName}function' : toLower('${lowerAppName}-func-${sanitizedEnvironment}') : environmentSpecificFunctionName
output functionAppName string            = functionAppName
output functionAppServicePlanName string = '${functionAppName}-${resourceAbbreviations.webServerFarms}'
output functionAppInsightsName string    = '${functionAppName}-${resourceAbbreviations.webSitesAppService}'

var functionFlexAppName = toLower('${sanitizedAppNameWithDashes}-${resourceAbbreviations.functionFlexApp}-${sanitizedEnvironment}')
output functionFlexAppName string        = functionFlexAppName
output functionFlexAppServicePlanName string = '${functionFlexAppName}-${resourceAbbreviations.webSitesAppService}'
output functionFlexInsightsName string   = '${functionFlexAppName}-${resourceAbbreviations.insightsComponents}'

output logAnalyticsWorkspaceName string  = toLower('${sanitizedAppNameWithDashes}-${sanitizedEnvironment}-${resourceAbbreviations.operationalInsightsWorkspaces}')
output cosmosDatabaseName string         = toLower('${sanitizedAppName}-${resourceAbbreviations.documentDBDatabaseAccounts}-${sanitizedEnvironment}')

output userAssignedIdentityName string   = toLower('${sanitizedAppName}-${resourceAbbreviations.managedIdentityUserAssignedIdentities}-${sanitizedEnvironment}')

// Key Vaults and Storage Accounts can only be 24 characters long
// Note - had to do an exception because I couldn't purge the old key vaults in prod which was in a different region...!
output keyVaultName string = environmentCode == 'prod' ? take('${sanitizedAppName}${resourceAbbreviations.keyVaultVaults}prd', 24) : take('${sanitizedAppName}${resourceAbbreviations.keyVaultVaults}-${sanitizedEnvironment}', 24)
output storageAccountName string         = take('${sanitizedAppName}${resourceAbbreviations.storageStorageAccounts}${sanitizedEnvironment}${dataStorageNameSuffix}', 24)
output functionStorageName string        = take('${sanitizedAppName}${resourceAbbreviations.storageStorageAccounts}${sanitizedEnvironment}${functionStorageNameSuffix}', 24)
output functionFlexStorageName string    = take('$${sanitizedAppName}${resourceAbbreviations.storageStorageAccounts}${sanitizedEnvironment}${functionFlexStorageNameSuffix}', 24)
