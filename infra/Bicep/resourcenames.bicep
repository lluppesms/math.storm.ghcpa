// --------------------------------------------------------------------------------
// Bicep file that builds all the resource names used by other Bicep templates
// --------------------------------------------------------------------------------
param appName string = ''
// @allowed(['azd','gha','azdo','dev','demo','qa','stg','ct','prod'])
param environmentCode string = 'azd'

// --------------------------------------------------------------------------------
var sanitizedEnvironment = toLower(environmentCode)
var sanitizedAppNameWithDashes = replace(replace(toLower(appName), ' ', ''), '_', '')
var sanitizedAppName = replace(replace(replace(toLower(appName), ' ', ''), '-', ''), '_', '')

// pull resource abbreviations from a common JSON file
var resourceAbbreviations = loadJsonContent('./data/resourceAbbreviations.json')

// --------------------------------------------------------------------------------
output logAnalyticsWorkspaceName string = toLower('${sanitizedAppNameWithDashes}-${sanitizedEnvironment}-${resourceAbbreviations.logWorkspaceSuffix}')
var webSiteName                         = toLower('${sanitizedAppNameWithDashes}-${sanitizedEnvironment}')
output webSiteName string               = webSiteName
output webSiteAppServicePlanName string = '${webSiteName}-${resourceAbbreviations.appServicePlanSuffix}'
output webSiteAppInsightsName string    = '${webSiteName}-${resourceAbbreviations.appInsightsSuffix}'
output sqlServerName string             = toLower('${sanitizedAppName}sql${sanitizedEnvironment}')

output userAssignedIdentityName string  = toLower('${sanitizedAppName}-app-${resourceAbbreviations.managedIdentity}')

output caManagedEnvName string          = toLower('${sanitizedAppName}-${resourceAbbreviations.containerAppEnvironment}-${sanitizedEnvironment}')

// CA name must be lower case alpha or '-', must start and end with alpha, cannot have '--', length must be <= 32
output containerAppUIName string        = take(toLower('${sanitizedAppName}-${resourceAbbreviations.containerApp}-ui-${sanitizedEnvironment}'), 32)

output containerRegistryName string     = take('${sanitizedAppName}${resourceAbbreviations.containerRegistry}${sanitizedEnvironment}', 50)

// Key Vaults and Storage Accounts can only be 24 characters long
output keyVaultName string              = take('${sanitizedAppName}${resourceAbbreviations.keyVaultAbbreviation}${sanitizedEnvironment}', 24)
output storageAccountName string        = take('${sanitizedAppName}${resourceAbbreviations.storageAccountSuffix}${sanitizedEnvironment}', 24)
