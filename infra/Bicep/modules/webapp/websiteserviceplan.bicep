// --------------------------------------------------------------------------------
// This BICEP file will create a Azure App Service Plan or use an existing one
// --------------------------------------------------------------------------------
@description('The name of the app service plan')
param appServicePlanName string = ''
@description('The name of a pre-existing app service plan')
param existingServicePlanName string = ''

param location string = resourceGroup().location
param environmentCode string = 'dev'
param commonTags object = {}
@allowed(['F1','B1','B2','S1','S2','S3'])
param sku string = 'B1'
param webAppKind string = 'linux'  // 'linux' or 'windows'

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~website.bicep'}
var azdTag = environmentCode == 'azd' ? { 'azd-service-name': 'web-plan' } : {}
var tags = union(commonTags, templateTag, azdTag)

// --------------------------------------------------------------------------------

resource existingAppServiceResource 'Microsoft.Web/serverfarms@2023-01-01' existing = if (!empty(existingServicePlanName)) {
  name: existingServicePlanName
}

resource appServiceResource 'Microsoft.Web/serverfarms@2023-01-01' = if (empty(existingServicePlanName)) {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: webAppKind
  properties: {
    reserved: webAppKind == 'linux' ? true : false
  }
}
output name string = empty(existingServicePlanName) ? appServiceResource.name : existingAppServiceResource.name
output id string = empty(existingServicePlanName) ? appServiceResource.id : existingAppServiceResource.id
