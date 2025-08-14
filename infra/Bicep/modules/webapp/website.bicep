// --------------------------------------------------------------------------------
// This BICEP file will create a Azure Website
// --------------------------------------------------------------------------------
param webSiteName string = ''
param location string = resourceGroup().location
param environmentCode string = 'dev'
param commonTags object = {}

@description('The workspace to store audit logs.')
param workspaceId string = ''

@description('The Name of the service plan to deploy into.')
param appServicePlanName string
param webAppKind string = 'linux' //  'linux' or 'windows'  (needs to be windows to use my shared app plan right now...)

@description('Shared Application Insights instrumentation key')
param sharedAppInsightsInstrumentationKey string

param managedIdentityId string
param managedIdentityPrincipalId string

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~website.bicep'}
var azdTag = environmentCode == 'azd' ? { 'azd-service-name': 'web' } : {}
var webSiteTags = union(commonTags, templateTag, azdTag)

// --------------------------------------------------------------------------------

resource appServiceResource 'Microsoft.Web/serverfarms@2023-01-01' existing = {
  name: appServicePlanName
}

resource webSiteResource 'Microsoft.Web/sites@2023-01-01' = {
  name: webSiteName
  location: location
  kind: 'app'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${managedIdentityId}': {} }
  }
  // identity: {
  //   type: 'SystemAssigned'
  // }
  tags: webSiteTags
  properties: {
    serverFarmId: appServiceResource.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      netFrameworkVersion: webAppKind == 'windows' ? 'v8.0' : null
      linuxFxVersion: webAppKind == 'linux' ? 'DOTNETCORE|8.0' : null
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      alwaysOn: true
      remoteDebuggingEnabled: false
      appSettings: [
        { 
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: sharedAppInsightsInstrumentationKey 
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${sharedAppInsightsInstrumentationKey}'
        }
      ]
    }
  }
}

resource webSiteAppSettings 'Microsoft.Web/sites/config@2023-01-01' = {
  parent: webSiteResource
  name: 'logs'
  properties: {
    applicationLogs: {
      fileSystem: {
        level: 'Warning'
      }
    }
    httpLogs: {
      fileSystem: {
        retentionInMb: 40
        enabled: true
      }
    }
    failedRequestsTracing: {
      enabled: true
    }
    detailedErrorMessages: {
      enabled: true
    }
  }
}

// can't seem to get this to work right... tried multiple ways...  keep getting this error:
//    No route registered for '/api/siteextensions/Microsoft.ApplicationInsights.AzureWebSites'.
// resource webSiteAppInsightsExtension 'Microsoft.Web/sites/siteextensions@2020-06-01' = {
//   parent: webSiteResource
//   name: 'Microsoft.ApplicationInsights.AzureWebSites'
//   dependsOn: [ appInsightsResource] or [ appInsightsResource, webSiteAppSettings ]
// }

resource webSiteMetricsLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${webSiteResource.name}-metrics'
  scope: webSiteResource
  properties: {
    workspaceId: workspaceId
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        // retentionPolicy: {
        //   days: 30
        //   enabled: true 
        // }
      }
    ]
  }
}

// https://learn.microsoft.com/en-us/azure/app-service/troubleshoot-diagnostic-logs
resource webSiteAuditLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${webSiteResource.name}-auditlogs'
  scope: webSiteResource
  properties: {
    workspaceId: workspaceId
    logs: [
      {
        category: 'AppServiceIPSecAuditLogs'
        enabled: true
        // retentionPolicy: {
        //   days: 30
        //   enabled: true 
        // }
      }
      {
        category: 'AppServiceAuditLogs'
        enabled: true
        // retentionPolicy: {
        //   days: 30
        //   enabled: true 
        // }
      }
    ]
  }
}

resource appServiceMetricLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${appServiceResource.name}-metrics'
  scope: appServiceResource
  properties: {
    workspaceId: workspaceId
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        // retentionPolicy: {
        //   days: 30
        //   enabled: true 
        // }
      }
    ]
  }
}
//output principalId string = webSiteResource.identity.principalId
output name string = webSiteName
output hostName string = webSiteResource.properties.defaultHostName
output webappAppPrincipalId string = managedIdentityPrincipalId
// Note: This will give you a warning saying it's not right, but it will contain the right value!
// output ipAddress string = webSiteResource.properties.inboundIpAddress 
