// --------------------------------------------------------------------------------
// This BICEP file will create storage account
// FYI: To purge a storage account with soft delete enabled: > az storage account purge --name storeName
// --------------------------------------------------------------------------------
param storageAccountName string = 'mystorageaccountname'
param location string = resourceGroup().location
param commonTags object = {}

// @allowed([ 'Standard_LRS', 'Standard_GRS', 'Standard_RAGRS' ])
param storageSku string = 'Standard_LRS'
param storageAccessTier string = 'Hot'
param containerNames array = ['input','output']
@description('Provide the IP address to allow access to the Azure Container Registry')
param myIpAddress string = ''

@description('Set to false to disable authorization with account access keys (Shared Key); forces Azure AD / SAS with user delegation.')
param allowSharedKeyAccess bool = false
param allowPublicNetworkAccess bool = true
param allowNetworkAccess bool = false

// --------------------------------------------------------------------------------
var templateTag = { TemplateFile: '~storageAccount.bicep' }
var tags = union(commonTags, templateTag)
// @allowed(['Enabled','Disabled'])
var publicNetworkAccess string = allowPublicNetworkAccess ? 'Enabled' : 'Disabled'
// @allowed(['Allow','Deny'])
var networkAccessSetting string = allowNetworkAccess ? 'Allow' : 'Deny'


// --------------------------------------------------------------------------------
resource storageAccountResource 'Microsoft.Storage/storageAccounts@2023-01-01' = {
    name: storageAccountName
    location: location
    sku: {
        name: storageSku
    }
    tags: tags
    kind: 'StorageV2'
    properties: {
        publicNetworkAccess: publicNetworkAccess
        networkAcls: {
            bypass: 'AzureServices'
            defaultAction: networkAccessSetting
            ipRules: empty(myIpAddress)
                ? []
                : [
                    {
                    value: myIpAddress
                    }
                ]
            virtualNetworkRules: []
            //virtualNetworkRules: ((virtualNetworkType == 'External') ? json('[{"id": "${subscription().id}/resourceGroups/${vnetResource}/providers/Microsoft.Network/virtualNetworks/${vnetResource.name}/subnets/${subnetName}"}]') : json('[]'))
        }
        supportsHttpsTrafficOnly: true
        encryption: {
            services: {
                file: {
                    keyType: 'Account'
                    enabled: true
                }
                blob: {
                    keyType: 'Account'
                    enabled: true
                }
            }
            keySource: 'Microsoft.Storage'
        }
        accessTier: storageAccessTier
        allowBlobPublicAccess: false
        minimumTlsVersion: 'TLS1_2'
    // When set to false, Shared Key authorization is disabled. Clients must use Azure AD (OAuth) or user delegation SAS.
    allowSharedKeyAccess: allowSharedKeyAccess
    }
}

resource blobServiceResource 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
    parent: storageAccountResource
    name: 'default'
    properties: {
        cors: {
            corsRules: [
            ]
        }
        deleteRetentionPolicy: {
            enabled: true
            days: 7
        }
    }
}

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [for containerName in containerNames: {
    name: '${containerName}'
    parent: blobServiceResource
    properties: {
      publicAccess: 'None'
      metadata: {}
    }
  }]


// --------------------------------------------------------------------------------
output id string = storageAccountResource.id
output name string = storageAccountResource.name
